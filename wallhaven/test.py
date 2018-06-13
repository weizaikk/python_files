import requests
from lxml import etree

def get_pagenum(keyword):
    url = 'https://alpha.wallhaven.cc/search?q={}&search_image='.format(keyword)
    response = requests.get(url)
    html = etree.HTML(response.text)
    pageInfo = str(html.xpath('//header[@class="listing-header"]/h1/i/text()'))
    print(pageInfo)
    # pagenum = pageInfo.split()[0]
    # return pagenum

get_pagenum('dog')