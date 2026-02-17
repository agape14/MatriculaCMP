using System;

/**
 * @author David PAXI
 */
namespace IDPeru.common
{
    public class Constants
    {
        //ACR
        public static String ACR_ONE_FACTOR = "one_factor";
        public static String ACR_TWO_FACTOR = "two_factor";
        public static String ACR_PKI_DNIE = "pki_dnie";
        public static String ACR_PKI_TOKEN = "pki_token";
        public static String ACR_ONLY_PASSWORD = "only_password";
        public static String ACR_ONLY_QUESTIONS = "only_questions";
        public static String PKI_DNIE_MOBILE = "pki_dnie_mobile";
        public static String FINGERPRINT_MOBILE = "fingerprint_mobile";
        public static String ACR_FACE_MOBILE = "face_mobile";
        public static String PKI_DNIE_LEGACY = "pki_dnie_legacy";
        public static String PKI_TOKEN_LEGACY = "pki_token_legacy";

        //PROMPT
        public static String PROMPT_NONE = "none";
        public static String PROMPT_LOGIN = "login";
        public static String PROMPT_CONSENT = "consent";

        //SCOPE
        public static String SCOPE_PROFILE = "profile";
        public static String SCOPE_EMAIL = "email";
        public static String SCOPE_PHONE = "phone";
        public static String SCOPE_OFFLINE_ACCESS = "offline_access";
    }
}
