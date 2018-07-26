#通过获取MongoDB中数据对主播直播间发送弹幕人的等级分布进行可视化比较
import matplotlib.pyplot as plt
import pymongo
import re

mongo_client = pymongo.MongoClient('localhost', 27017)
db = mongo_client.Douyu_danmu
col_msg1 = db.MQ
col_msg2 = db.YT

#正则匹配发送弹幕人的等级
levels_MQ = col_msg1.find({'sender_level': re.compile('\d+')})
levels_YT = col_msg2.find({'sender_level': re.compile('\d+')})
level_list_MQ = []
level_list_YT = []
nums_MQ = []
xlabel_MQ = []
nums_YT = []
xlabel_YT = []

for level in levels_MQ:
    level_list_MQ.append(int(level['sender_level']))
for value in range(1, max(level_list_MQ) + 1):
    num = level_list_MQ.count(value)
    xlabel_MQ.append(value)
    nums_MQ.append(num)

for level in levels_YT:
    level_list_YT.append(int(level['sender_level']))
for value in range(1, max(level_list_YT) + 1):
    num = level_list_YT.count(value)
    xlabel_YT.append(value)
    nums_YT.append(num)

plt.figure(dpi=128, figsize=(10, 6))
plt.plot(xlabel_MQ, nums_MQ, linewidth=1, c='black')
plt.plot(xlabel_YT, nums_YT, linewidth=1, c='red')
plt.title("the distribution of barrage_sender's level", fontsize=16)
plt.xlabel('level')
plt.ylabel('number')
plt.show()
