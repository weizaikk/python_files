# 多线程 反爬措施 日志模块
import requests
import pymongo
import json
import time
import random

mongo_client = pymongo.MongoClient('localhost', 27017)
db = mongo_client.lagou_db
col_msg = db.lagou_shanghai

keyword = input('please enter the position you want to search for: ')
city_name = input('please enter the city: ')
url = 'https://www.lagou.com/jobs/positionAjax.json?px=default&city={}&needAddtionalResult=false'.format(city_name)
headers = {
    'User-Agent': 'Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36',
    'Referer': 'https://www.lagou.com/jobs/list_python?labelWords=&fromSearch=true&suginput=',
    'Origin': 'https://www.lagou.com',
    'Host': 'www.lagou.com',
    'Accept': 'application/json, text/javascript, */*; q=0.01',
    'Accept-Encoding': 'gzip, deflate, br',
    'Accept-Language': 'zh-CN,zh;q=0.9',
    'Connection': 'keep-alive',
    'Content-Length': '25',
    'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
    'Cookie': '_ga=GA1.2.1156340859.1521775253; user_trace_token=20180323112052-314c731e-2e49-11e8-9989-525400f775ce; LGUID=20180323112052-314c77c7-2e49-11e8-9989-525400f775ce; WEBTJ-ID=20180803165047-164fefb6af36dd-08c7c8a48da7ad-3e3d5f01-1049088-164fefb6af46a4; Hm_lvt_4233e74dff0ae5bd0a3d81c6ccf756e6=1533286248; X_HTTP_TOKEN=bb862545bcb74d1ae72160159a969e13; LG_LOGIN_USER_ID=90e92faf17070dc1446400d19758324df67fc1f6a65df914; _putrc=F7FD1B012897DDD0; JSESSIONID=ABAAABAABEEAAJAADD681F9127C8A5A3D1A84AE0D64DE80; login=true; unick=%E7%8E%8B%E5%BF%97%E7%82%9C; showExpriedIndex=1; showExpriedCompanyHome=1; showExpriedMyPublish=1; hasDeliver=8; index_location_city=%E5%85%A8%E5%9B%BD; _gid=GA1.2.655296297.1533524485; gate_login_token=69b77798d95caed125f692a2ed5fd824e334a09e2bb02fd8; TG-TRACK-CODE=index_search; Hm_lpvt_4233e74dff0ae5bd0a3d81c6ccf756e6=1533607635; LGRID=20180807100729-a2f4a4f3-99e6-11e8-b74e-525400f775ce; SEARCH_ID=392bca6f2c424477b5a37f87b8a116ff',
    'X-Anit-Forge-Code': '0',
    'X-Anit-Forge-Token': 'None',
    'X-Requested-With': 'XMLHttpRequest',
}


def get_total_page():
    data_post = {
        'kd': keyword,
    }
    response = requests.post(url=url, headers=headers, data=data_post)
    text = json.loads(response.text)
    total_count = text['content']['positionResult']['totalCount']
    if total_count / 15 == 0:
        pages = int(total_count / 15)
    else:
        pages = int(total_count / 15) + 1
    return pages


def get_result(page_number):
    data_post = {
        'kd': keyword,
        'pn': page_number,
    }
    response = requests.post(url=url, headers=headers, data=data_post)
    text = json.loads(response.text)
    for result in text['content']['positionResult']['result']:
        job_message = {
            'company_name': result['companyFullName'],
            'position_name': result['positionName'],
            'salary': result['salary'],
            'city': result['city'],
            'work_year': result['workYear'],
            'education': result['education'],
            'company_district': result['district'],
            'position_advantage': result['positionAdvantage'],
            'company_size': result['companySize'],
        }
        print(job_message)
        col_msg.insert_one(job_message)


if __name__ == '__main__':
    total_page = get_total_page()
    for page_num in range(1, total_page + 1):
        get_result(page_num)
        time.sleep(random.randint(2, 4))
