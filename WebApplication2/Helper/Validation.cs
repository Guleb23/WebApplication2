namespace WebApplication2.Helper
{
    public class Validation
    {
        public string ValidPhone(string phone)
        {
            char leftSkob = ')'; 
            char rightSkob = '(';
            char probel = ' ';
            char minus = '-';
            string result = "";

            for (int i = 0; i < phone.Length; i++)
            {
                if (phone[i] != leftSkob && phone[i] != rightSkob && phone[i] != probel && phone[i] != minus)
                {
                    result += phone[i];
                }
            }
            return result;
        }
    }
}
