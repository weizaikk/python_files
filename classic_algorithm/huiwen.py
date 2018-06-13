class Solution:
    def isPalindrome(self, x):
        if x < 0:
            return None
        while x:
            if str(x) == str(x)[::-1]:
                return True
            else:
                return False


p = Solution()
print(p.isPalindrome(1334564331))
