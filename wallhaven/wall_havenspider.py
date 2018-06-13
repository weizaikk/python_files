import requests
import os
import time
import re
from lxml import etree
# from threading import Thread

keyword = input('please input the keyword you want to search:')


class Spider():
    def __init__(self):
        self.headers = {
            "user-agent": "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36"}
        self.Filepath = 'E:/python_project/wallhaven/' + keyword + '/'

    def create_file(self):                             #文件保存路径
        Filepath = self.Filepath
        if not os.path.exists(Filepath):
            os.makedirs(Filepath)

    def get_pagenum(self):                             #获取搜索到的图片数量
        url = 'https://alpha.wallhaven.cc/search?q={}&search_image='.format(keyword)
        response = requests.get(url)
        html = etree.HTML(response.text)
        pageInfo = str(html.xpath('//header[@class="listing-header"]/h1/text()'))
        pagenum = re.sub("\D", "", pageInfo)
        return pagenum

    def get_piclinks(self, number):                    #图片URL地址
        url = 'https://alpha.wallhaven.cc/search?q={}&search_image=&page={}'.format(keyword, number)
        response = requests.get(url, headers=self.headers)
        html = etree.HTML(response.text)
        piclinks = html.xpath('//a[@class="jsAnchor thumb-tags-toggle tagged"]/@href')
        return piclinks

    def load_pics(self, url):                            #下载图片
        picindex = url.split("/")[4]
        pic_path = self.Filepath + keyword + picindex + ".jpg"
        imageurl = 'https://alpha.wallhaven.cc/wallpapers/thumb/small/th-{}.jpg'.format(picindex)
        start = time.time()
        pic = requests.get(imageurl, headers=self.headers)
        with open(pic_path, 'wb') as f:
            f.write(pic.content)
            f.close()
        end = time.time()
        costtime = end - start
        print("Image {} has been download,cost: {}".format(picindex, costtime))

    def main(self):
        self.create_file()
        count = int(self.get_pagenum())
        print('We have found {} images!'.format(count))
        pages = int(count / 24) + 1
        start = time.time()
        for i in range(pages):
            urls = self.get_piclinks(i + 1)
            for url in urls:
                # threads = []
                # t = Thread(target=self.load_pics, args=url)
                # t.start()
                # threads.append(t)
                self.load_pics(url)
            # for t in threads:
            #     t.join()
        end = time.time()
        cost = end - start
        print('Total cost time is {}'.format(cost))


spider = Spider()
spider.main()
