import requests
import json

room_id = input('请输入你想要接入的主播房间号')
url = 'http://open.douyucdn.cn/api/RoomApi/room/{}'.format(room_id)
text = requests.get(url)
res = text.json()
print(res)
