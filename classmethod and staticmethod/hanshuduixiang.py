class A(object):
    def m1(self):
        print('self:', self)
    @classmethod
    def m2(cls, n):
        print('cls:', cls)
    @staticmethod
    def m3():
        pass
a = A()
a.m1()
A.m2(1)
A.m3()
