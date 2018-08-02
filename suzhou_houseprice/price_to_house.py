# 多线程、热力图分析、房价集中区域
import requests
from lxml import etree
import pymongo
import re

client_mongo = pymongo.MongoClient('localhost', 27017)
db = client_mongo.area_su
col_msg = db.area_message_su


def get_areas():
    url = 'https://su.ke.com/ershoufang/'
    resp = requests.get(url)
    text = resp.content.decode('utf-8')
    html = etree.HTML(text)
    area_nodes = html.xpath('.//div[@class="m-filter"]//div[@data-role="ershoufang"]/div/a')
    area_dict = {}
    for area in area_nodes:
        area_url = 'https://su.ke.com' + area.attrib['href']
        sub_resp = requests.get(area_url)
        sub_text = sub_resp.content.decode('utf-8')
        sub_html = etree.HTML(sub_text)
        subarea_nodes = sub_html.xpath('.//div[@class="m-filter"]//div[@data-role="ershoufang"]/div')[1].xpath('./a')
        for item in subarea_nodes:
            sub_url = 'https://su.ke.com' + item.attrib['href']
            area_dict[area.text + item.text] = sub_url
    return area_dict


def get_total_page(url):
    resp = requests.get(url)
    content = resp.content.decode('utf-8')
    html = etree.HTML(content)
    try:
        str = html.xpath('.//div[@class="content "]//div[@class="page-box fr"]/div/@page-data')[0].encode('utf-8')
        total_page = re.match('^{"totalPage":(\d+)', str.decode('utf-8'))
        return total_page.group(1)
    except:

        return 0


def get_price():
    area_dict = get_areas()
    for area, url in area_dict.items():
        pages = int(get_total_page(url))
        for i in range(1, pages + 1):
            all_url = url + 'pg{}'.format(i)
            resp = requests.get(all_url)
            content = resp.content.decode('utf-8')
            html = etree.HTML(content)
            xiaoqu_nodes = html.xpath('.//div[@class="content "]//div[@class="houseInfo"]/a/text()')
            message_nodes = html.xpath('.//div[@class="content "]//div[@class="houseInfo"]/text()')
            unitprice_nodes = html.xpath('.//div[@class="content "]//div[@class="unitPrice"]/span/text()')
            totalprice_nodes = html.xpath('.//div[@class="content "]//div[@class="totalPrice"]/span/text()')
            for j in range(len(xiaoqu_nodes)):
                text = {
                    'area': area,
                    'xiaoqu': xiaoqu_nodes[j],
                    'message': message_nodes[j],
                    'unit_price': unitprice_nodes[j],
                    'total_price': totalprice_nodes[j],
                }
                print(text)
                col_msg.insert_one(text)


get_price()