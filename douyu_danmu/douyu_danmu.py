# 爬取添加内容，主播的开播详情，返回错误信息提示，进行数据分析，发弹幕人的等级分析,词云
import socket
import pymongo
import re
import time
import requests
import json
import multiprocessing
import jieba
from wordcloud import WordCloud, ImageColorGenerator
from scipy.misc import imread
import matplotlib.pyplot as plt

# 连接到MongoDB数据库
client_mongo = pymongo.MongoClient('localhost', 27017)
db = client_mongo.Douyu_danmu
col_msg = db.text

# socket通讯建立
client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
host = socket.gethostbyname('openbarrage.douyutv.com')
port = 8601
client.connect((host, port))

# 词云的背景图片
back_color = imread('Penguins.jpg')

# socket接收内容的正则pattern
danmu_pattern = re.compile(b'txt@=(.+?)/cid@')
uid_pattern = re.compile(b'uid@=(.+?)/nn@')
nickname_pattern = re.compile(b'/nn@=(.+?)/')
level_pattern = re.compile(b'level@=([1-9][0-9]?)/')


# 发送数据包
def send_packet(pac_content):
    code = 689
    packet_bytes = pac_content.encode('utf-8')
    data_length = len(packet_bytes) + 8
    packet_head = int.to_bytes(data_length, 4, 'little') + int.to_bytes(data_length, 4, 'little') + \
                  int.to_bytes(code, 4, 'little')
    client.send(packet_head)
    sent = 0
    while sent < len(packet_bytes):
        tn = client.send(packet_bytes[sent:])
        sent = sent + tn


# 通过API获取主播昵称
def get_zb_name(room_id):
    url = 'http://open.douyucdn.cn/api/RoomApi/room/{}'.format(room_id)
    resp = requests.get(url)
    data = json.loads(resp.text)
    status_code = data['data']['room_status']
    if data['error'] == 0:
        zbname = data['data']['owner_name']
        print('#####################欢迎来到{}的直播间####################'.format(zbname))
        if status_code == 2:
            print('该主播现在未开播')
    elif data['error'] == 999:
        print('API接口维护中，暂不可用')
    else:
        print('Something wrong!')


# 定时发送给服务期端的数据包，维持与后台的连接
def keeplive():
    pac_content = 'type@=keeplive/tick@={}\0'.format(str(time.time()))
    send_packet(pac_content)
    time.sleep(15)


def get_response(room_id):
    pac_content1 = 'type@=loginreq/roomid@={}/\0'.format(str(room_id))
    send_packet(pac_content1)
    pac_content2 = 'type@=joingroup/rid@={}/gid@=-9999\0'.format(room_id)
    send_packet(pac_content2)
    get_zb_name(room_id)
    while True:
        data_pac = client.recv(1024)
        danmu = danmu_pattern.findall(data_pac)
        uid = uid_pattern.findall(data_pac)
        nickname = nickname_pattern.findall(data_pac)
        level = level_pattern.findall(data_pac)
        if not level:
            level = b'0'
        if not data_pac:
            break
        else:
            for i in range(0, len(danmu)):
                try:
                    text = {
                        'danmu': danmu[0].decode(encoding='utf-8'),
                        'sender_uid': uid[0].decode(encoding='utf-8'),
                        'sender_nickname': nickname[0].decode(encoding='utf-8'),
                        'sender_level': level[0].decode(encoding='utf-8'),
                        'write_time': str(time.strftime('%Y-%m-%d %H:%M:%S', time.localtime())),
                    }
                    print(text)
                    # 避免在数据库中插入重复的内容
                    if not col_msg.find_one({'danmu': text['danmu'], 'sender_uid': text['sender_uid']}):
                        col_msg.insert(text)
                        print('成功保存到MongoDB->Douyu_danmu->text')
                except Exception as e:
                    print(e)


# 获取生成词云所需要的单词
def get_words_freq(col_msg):
    results = col_msg.find()
    word_list = []
    mask_list = []
    for result in results:
        barrage = result['danmu']
        words = jieba.cut(str(barrage), cut_all=False)
        #将一些无用的符号去除
        with open('filter.txt', encoding='utf-8') as f:
            lines = f.readlines()
            for line in lines:
                for mask_word in line:
                    mask_list.append(mask_word)
        for word in words:
            if word not in mask_list:
                word_list.append(word)
        # yield word_list
    return ' '.join(word_list)


# 生成词云
def draw_wordcloud():
    text = get_words_freq(col_msg)
    wc = WordCloud(
        font_path='C:/Windows/Fonts/simfang.ttf',
        background_color='white',
        max_words=100,
        max_font_size=100,
        mask=back_color,
        random_state=42,
    )
    wc.generate(text)
    image_colors = ImageColorGenerator(back_color)
    plt.imshow(wc)
    plt.axis('off')
    plt.figure()
    plt.imshow(wc.recolor(color_func=image_colors))
    plt.axis('off')
    wc.to_file('word_cloud.jpg')


if __name__ == '__main__':
    room_id = input('请输入你想要连接的直播房间号:  ')
    p1 = multiprocessing.Process(target=get_response, args=(room_id,))
    p2 = multiprocessing.Process(target=keeplive)
    p1.start()
    p2.start()
    flag = input('是否生成词云(Y/N)\n')
    if flag == 'Y':
        draw_wordcloud()
    



