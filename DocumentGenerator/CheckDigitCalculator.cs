using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator
{
    public static class MrzGeneration
    {
        public static string GeneratePassportMRZ(PassportModel passport)
        {

            passport.PersonalNum = passport.PersonalNum.Replace(' ', '<').PadRight(14, '<');

            Dictionary<string, string> result = new Dictionary<string, string>
            {
                { "passport", mrzCheckDigitConvert(passport.PassportNum).ToString() },
                { "dob", mrzCheckDigitConvert(passport.Dob).ToString() },
                { "expDate", mrzCheckDigitConvert(passport.ExpDate).ToString() },
                { "personalNum", mrzCheckDigitConvert(passport.PersonalNum).ToString() }
            };
            string final = passport.PassportNum + result["passport"].ToString() + passport.Dob + result["dob"].ToString() + passport.ExpDate + result["expDate"].ToString() + passport.PersonalNum + result["personalNum"].ToString();
            result.Add("final", mrzCheckDigitConvert(final).ToString());

            string MRZ1;
            string name = passport.SurName.ToUpper() + "<<" + passport.GivenNames.Replace(' ', '<').Replace('-', '<').ToUpper();
            if (name.Length > 39)
            {
                name = name.Substring(0, 39);
            }
            MRZ1 = passport.DocType.PadRight(2,'<').ToUpper()+passport.CountryOfIssue;
            MRZ1 += name;
	        MRZ1 = MRZ1.PadRight(44,'<');
              
            string total = passport.PassportNum + result["passport"].ToString() + passport.Nationality.ToString() + passport.Dob + result["dob"].ToString() + 
                Convert.ToChar(Convert.ChangeType(passport.Sex,passport.Sex.GetTypeCode()))
                + passport.ExpDate + result["expDate"].ToString() + passport.PersonalNum + result["personalNum"] + result["final"].ToString();
            //total.Dump("Total");
            result.Add("mrz", total);

            return MRZ1+Environment.NewLine+total;
        }
        
        
        public static string[] GenerateIdentityCardMRZ(IdentityDocumentModel document)
        {
            string[] MRZ = new string[3];
            string lineOne = document.DocType.PadRight(2, '<');
            lineOne += document.CountryOfIssue;
            lineOne += document.DocumentNum;
            lineOne += mrzCheckDigitConvert(document.DocumentNum);
            lineOne += document.OptionalOne;
            lineOne = lineOne.PadRight(30, '<').Substring(0, 30);

            string lineTwo = document.Dob;
            lineTwo += mrzCheckDigitConvert(document.Dob);
            lineTwo += Convert.ToChar(Convert.ChangeType(document.Sex, document.Sex.GetTypeCode()));
            lineTwo += document.ExpDate;
            lineTwo += mrzCheckDigitConvert(document.ExpDate);
            lineTwo += document.Nationality;
            lineTwo += document.OptionalTwo;
            lineTwo = lineTwo.PadRight(30, '<').Substring(0, 29);
            lineTwo += mrzCheckDigitConvert(lineOne.Substring(5,25)+lineTwo.Substring(0,7)+lineTwo.Substring(8,7)+lineTwo.Substring(18,10));

            string lineThree = (document.SurName + "<<" + document.GivenNames).PadRight(30, '<').Substring(0, 30);

            MRZ[0] = lineOne;
            MRZ[1] = lineTwo;
            MRZ[2] = lineThree;

            return MRZ; //lineOne + Environment.NewLine + lineTwo + Environment.NewLine + lineThree;
        }


        private static int mrzCheckDigitConvert(string phrase)
        {
            int result = 0;
            int count = 0;
            Dictionary<char, int> mrzCheckDigitLookup = new Dictionary<char, int>
            {
                { '<', 0 },
                { 'A', 10 },
                { 'B', 11 },
                { 'C', 12 },
                { 'D', 13 },
                { 'E', 14 },
                { 'F', 15 },
                { 'G', 16 },
                { 'H', 17 },
                { 'I', 18 },
                { 'J', 19 },
                { 'K', 20 },
                { 'L', 21 },
                { 'M', 22 },
                { 'N', 23 },
                { 'O', 24 },
                { 'P', 25 },
                { 'Q', 26 },
                { 'R', 27 },
                { 'S', 28 },
                { 'T', 29 },
                { 'U', 30 },
                { 'V', 31 },
                { 'W', 32 },
                { 'X', 33 },
                { 'Y', 34 },
                { 'Z', 35 }
            };

            foreach (var letter in phrase.Replace(' ','<').ToUpper().ToCharArray())
            {
                count++;

                int charValue;
                if (Char.IsNumber(letter))
                {
                    charValue = (int)char.GetNumericValue(letter);
                }
                else if (mrzCheckDigitLookup.ContainsKey(letter))
                {
                    charValue = mrzCheckDigitLookup[letter];
                }
                else
                {
                    var ex = new Exception("Unknown character in MRZ :" + letter);
                    throw ex;
                }

                var weightedcharValue = WeightingMultiplier(count, charValue);
                result += weightedcharValue;
                var tmp = " " + letter + "[" + charValue + "] * " + WeightingMultiplier(count, 1) + " = " + weightedcharValue + " += " + result;
                if (count > 2)
                {
                    count = 0;
                }
                //tmp.Dump();
            }

            var remainder = result % 10;
            //(result+" % 10 = "+remainder+Environment.NewLine).Dump();

            return remainder;
        }

        private static int WeightingMultiplier(int count, int value)
        {
            switch (count)
            {
                case 1:
                    value *= 7;
                    break;
                case 2:
                    value *= 3;
                    break;
                case 3:
                    value *= 1;
                    break;
            }

            return value;
        }
    }
}
