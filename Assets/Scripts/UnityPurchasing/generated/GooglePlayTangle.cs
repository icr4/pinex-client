// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("Svonn576xMLtNF102rS4LNF6PCl6+ff4yHr58vp6+fn4MDqhvkgWwGmApTOo3bUNKwIDnV+XuygOd0eX7GT77Mrp/VyrCbfPw6VHb8Q0HS+26Vc8Fs20+15T368Gw4zVjy+h9zF498cOxqUfvlSy61J8cBp6DkszqROTPVwDUMrXJagJvP3iv+SRpfg/e1N1IG/Cn1jja1SgH7fOAKsGPgGpdH+IIaoHvTy8un3egR2U6uog9OKtXO6V2WLaFgUs2J4sXrRMs9IhoIa2lIojQ6MOC6VGoEjma6n6K2bPpPSpgbz1Gk39uOLxMMKWBdHLyHr52sj1/vHSfrB+D/X5+fn9+PtACLtTYrql3kNRcoQgjAC4LCXpr+K7ovtA40Its/r7+fj5");
        private static int[] order = new int[] { 7,1,13,12,8,5,10,9,11,13,12,12,13,13,14 };
        private static int key = 248;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
