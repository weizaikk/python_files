#将发送弹幕者的发送频率进行可视化比较
import matplotlib.pyplot as plt
import pymongo
import re

mongo_client = pymongo.MongoClient('localhost', 27017)
db = mongo_client.Douyu_danmu
col_msg1 = db.MQ
col_msg2 = db.YT

#正则匹配数据库中的发送弹幕人的昵称
nns_MQ = col_msg1.find({'sender_nickname': re.compile('.*?')})
nns_YT = col_msg2.find({'sender_nickname': re.compile('.*?')})

nn_list_MQ, num_list_MQ = [], []
nn_list_YT, num_list_YT = [], []

for nn in nns_MQ:
    nn_list_MQ.append(nn['sender_nickname'])
for value in set(nn_list_MQ):
    num_list_MQ.append(nn_list_MQ.count(value))
num_list_MQ.sort(reverse=True)

for nn in nns_YT:
    nn_list_YT.append(nn['sender_nickname'])
for value in set(nn_list_YT):
    num_list_YT.append(nn_list_YT.count(value))
num_list_YT.sort(reverse=True)

plt.figure(dpi=128, figsize=(10, 6))
plt.plot(num_list_MQ, linewidth=1, c='black')
plt.plot(num_list_YT, linewidth=1, c='red')
plt.title("the distribution of barrage_sender", fontsize=16)
plt.ylabel('barrage number')
plt.show()
