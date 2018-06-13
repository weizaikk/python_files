import matplotlib.pyplot as plt
from random_walk import Randomwalk

rw = Randomwalk()
rw.fill_walk()
numwalks = list(range(rw.num_walks))
plt.scatter(rw.x_values, rw.y_values, c=numwalks, cmap=plt.cm.Blues, s=15)
plt.show()
