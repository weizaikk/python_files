#爬取添加内容，主播的开播详情，返回错误信息提示，进行数据分析，发弹幕人的等级水军问题
import socket
import pymongo
import re
import time
import requests
from bs4 import BeautifulSoup

client_mongo = pymongo.MongoClient('localhost')
db = client_mongo.Douyu_danmu
col_msg = db.text

client = socket.socket(socket.AF_INET, socket.SOC_STREAM)
host = 'openbarrage.douyutv.com'
port = 8601
client.connect((host, port))

path_danmu = re.compile(b'txt@=(.+?)/cid@')
path_uid = re.compile(b'uid@=(.+?)/nn@')
path_nickname = re.compile(b'nn@=(.+?)/txt@')
path_level = re.compile(b'level@=([1-9][0-9]?)/sahf')


def send_packet(pac_content):
    code = 689
    packet_bytes = pac_content.encode('utf-8')
    data_length = len(packet_bytes) + 8
    packet_head = int.to_bytes(data_length, 4, 'little') + int.to_bytes(data_length, 4, 'little') + \
                  int.to_bytes(code, 4, 'little')
    client.send(packet_head)
    sent = 0
    while sent < len(pac_content):
        tn = client.send(pac_content[sent:])
        sent = sent + tn


def get_zbname(room_id):
    resp = requests.get("http://www.douyu.com/" + room_id)
    text = BeautifulSoup(resp.text, 'lxml')
    zbname = text.find('a', {'class', 'zb-name'}).string
    return zbname


def get_response(room_id):
    pac_content1 = 'type@=loginreq/roomid@={}/\0'.format(str(room_id))
    send_packet(pac_content1)
    pac_content2 = 'type@=keeplive/tick@={}'.format(str(time.time()))
    send_packet(pac_content2)
    pac_content3 = 'type@=joingroup/rid@={}/gid@=-9999'.format(room_id)
    send_packet(pac_content3)
    print('欢迎进入{}的直播间，实时弹幕如下:'.format(get_zbname(room_id)))
    while True:
        data_pac = client.recv(1024)
        danmu = path_danmu.findall(data_pac)
        uid = path_uid.findall(data_pac)
        nickname = path_nickname.findall(data_pac)
        level = path_level.findall(data_pac)
        if not level:
            level = b'0'
        if not data_pac:
            break
        else:
            for i in range(0, len(danmu)):
                text = {
                    'danmu': danmu[0].decode(encoding='utf-8'),
                    'uid': uid[0].decode(encoding='utf-8'),
                    'nickname': nickname[0].decode(encoding='utf-8'),
                    'level': level[0].decode(encoding='utf-8'),
                }
                print(text)
                col_msg.insert(text)
                print('成功保存到MongoDB')
