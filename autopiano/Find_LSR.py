import csv
import matplotlib.pyplot as plt
import numpy as np

Midi2Key = dict()
Midi2Tone = dict()
music = list()
KeyOn = list()

with open("Midi2Key.txt", 'r') as f:
    a = csv.reader(f)
    for row in a:
        if len(row) < 3:
            continue
        elif int(row[2]) > 0:
            Midi2Key.update({int(row[0]): int(row[2])})
            Midi2Tone.update({int(row[0]): int(row[1])})

# for k, v in Midi2Tone.items():
#     print(k,v)


with open("mmh.csv", 'r') as f:
    a = csv.reader(f)
    for row in a:
        if len(row) < 6:
            continue
        elif row[4] == "NoteOff":
            ls = list([int(row[5]), int(row[1])])
            music.append(ls)

for ls in music:
    ls.append(Midi2Tone[ls[0]])
    ls.append(Midi2Key[ls[0]])
    KeyOn.append(Midi2Key[ls[0]])
print(music)
print(KeyOn)

list2 = KeyOn[0:len(KeyOn) // 2]
print(list2)
plt.tick_params(axis='both', labelsize=5)
plt.xticks(np.arange(0, len(KeyOn) // 2, 1))
plt.yticks(np.arange(20, 35, 1))
plt.grid()
plt.plot(list2)
plt.show()

# KeyOn:曲目对应要按下的琴键链表
# finger:中间手指对应的琴键位置
# Mover:手指每次移动的目标琴键位置
# tmp:当前进行匹配的缓存链表
# tt:包含最新的按键的链表
# FingerKey:每个音调所对应的中指对应的琴键位置

n = KeyOn[0]
finger = n
Mover = list()
tmp = list()
FingerKey = list()
for x in KeyOn:
    if len(tmp) > 0:
        tt = tmp.copy()
        tt.append(x)
        n = max(tt) - min(tt)

        if n > 4:
            finger = (max(tmp) + min(tmp)) / 2
            Mover.append(finger)
            for y in tmp:
                FingerKey.append(finger)
            tmp.clear()
            tmp.append(x)
        else:
            tmp.append(x)
    else:
        tmp.append(x)
if len(FingerKey) < len(KeyOn):
    finger = (max(tmp) + min(tmp)) / 2
    Mover.append(finger)
    for y in tmp:
        FingerKey.append(finger)

print(KeyOn)
print(FingerKey)
print(Mover)

count = 0
base = Mover[0]
Mover_offset = list()
for x in Mover:
    Mover_offset.append(x - base)
    count += abs(x - base)
    base = x
print(Mover_offset)
print('总共有%2d个音调，共移动%2d次，移动距离%2d个琴键' % (len(KeyOn), len(Mover), count))
