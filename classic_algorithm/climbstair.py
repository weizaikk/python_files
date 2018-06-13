def climb_stair(number):
    if number <=2:
        return number
    currt = 2
    prevt = 1
    for _ in range(3, number + 1):
        currt, prevt  = prevt + currt, currt
        
    return currt

curr = climb_stair(5)
print(curr)

