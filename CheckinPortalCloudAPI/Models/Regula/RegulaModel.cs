using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Models.Regula
{
    public class RegulaDocumentType
    {


        public const int NOT_DEFINED = 0;


        public const int PASSPORT = 11;


        public const int IDENTITY_CARD = 12;


        public const int DIPLOMATIC_PASSPORT = 13;


        public const int SERVICE_PASSPORT = 14;


        public const int SEAMANS_IDENTITY_DOCUMENT = 15;


        public const int IDENTITY_CARD_FOR_RESIDENCE = 16;


        public const int TRAVEL_DOCUMENT = 17;


        public const int OTHER = 99;


        public const int VISA_ID2 = 29;


        public const int VISA_ID3 = 30;


        public const int NATIONAL_IDENTITY_CARD = 20;


        public const int SOCIAL_IDENTITY_CARD = 21;


        public const int ALIENS_IDENTITY_CARD = 22;


        public const int PRIVILEGED_IDENTITY_CARD = 23;


        public const int RESIDENCE_PERMIT_IDENTITY_CARD = 24;


        public const int ORIGIN_CARD = 25;


        public const int EMERGENCY_PASSPORT = 26;


        public const int ALIENS_PASSPORT = 27;


        public const int ALTERNATIVE_IDENTITY_CARD = 28;


        public const int AUTHORIZATION_CARD = 32;


        public const int BEGINNER_PERMIT = 33;


        public const int BORDER_CROSSING_CARD = 34;


        public const int CHAUFFEUR_LICENSE = 35;


        public const int CHAUFFEUR_LICENSE_UNDER_18 = 36;


        public const int CHAUFFEUR_LICENSE_UNDER_21 = 37;


        public const int COMMERCIAL_DRIVING_LICENSE = 38;


        public const int COMMERCIAL_DRIVING_LICENSE_INSTRUCTIONAL_PERMIT = 39;


        public const int COMMERCIAL_DRIVING_LICENSE_UNDER_18 = 40;


        public const int COMMERCIAL_DRIVING_LICENSE_UNDER_21 = 41;


        public const int COMMERCIAL_INSTRUCTION_PERMIT = 42;


        public const int COMMERCIAL_NEW_PERMIT = 43;


        public const int CONCEALED_CARRY_LICENSE = 44;


        public const int CONCEALED_FIREARM_PERMIT = 45;


        public const int CONDITIONAL_DRIVING_LICENSE = 46;


        public const int DEPARTMENT_OF_VETERANS_AFFAIRS_IDENTITY_CARD = 47;


        public const int DIPLOMATIC_DRIVING_LICENSE = 48;


        public const int DRIVING_LICENSE = 49;


        public const int DRIVING_LICENSE_INSTRUCTIONAL_PERMIT = 50;


        public const int DRIVING_LICENSE_INSTRUCTIONAL_PERMIT_UNDER_18 = 51;


        public const int DRIVING_LICENSE_INSTRUCTIONAL_PERMIT_UNDER_21 = 52;


        public const int DRIVING_LICENSE_LEARNERS_PERMIT = 53;


        public const int DRIVING_LICENSE_LEARNERS_PERMIT_UNDER_18 = 54;


        public const int DRIVING_LICENSE_LEARNERS_PERMIT_UNDER_21 = 55;


        public const int DRIVING_LICENSE_NOVICE = 56;


        public const int DRIVING_LICENSE_NOVICE_UNDER_18 = 57;


        public const int DRIVING_LICENSE_NOVICE_UNDER_21 = 58;


        public const int DRIVING_LICENSE_REGISTERED_OFFENDER = 59;


        public const int DRIVING_LICENSE_RESTRICTED_UNDER_18 = 60;


        public const int DRIVING_LICENSE_RESTRICTED_UNDER_21 = 61;


        public const int DRIVING_LICENSE_TEMPORARY_VISITOR = 62;


        public const int DRIVING_LICENSE_TEMPORARY_VISITOR_UNDER_18 = 63;


        public const int DRIVING_LICENSE_TEMPORARY_VISITOR_UNDER_21 = 64;


        public const int DRIVING_LICENSE_UNDER_18 = 65;


        public const int DRIVING_LICENSE_UNDER_21 = 66;


        public const int EMPLOYMENT_DRIVING_PERMIT = 67;


        public const int ENHANCED_CHAUFFEUR_LICENSE = 68;


        public const int ENHANCED_CHAUFFEUR_LICENSE_UNDER_18 = 69;


        public const int ENHANCED_CHAUFFEUR_LICENSE_UNDER_21 = 70;


        public const int ENHANCED_COMMERCIAL_DRIVING_LICENSE = 71;


        public const int ENHANCED_DRIVING_LICENSE = 72;


        public const int ENHANCED_DRIVING_LICENSE_UNDER_18 = 73;


        public const int ENHANCED_DRIVING_LICENSE_UNDER_21 = 74;


        public const int ENHANCED_IDENTITY_CARD = 75;


        public const int ENHANCED_IDENTITY_CARD_UNDER_18 = 76;


        public const int ENHANCED_IDENTITY_CARD_UNDER_21 = 77;


        public const int ENHANCED_OPERATORS_LICENSE = 78;


        public const int FIREARMS_PERMIT = 79;


        public const int FULL_PROVISIONAL_LICENSE = 80;


        public const int FULL_PROVISIONAL_LICENSE_UNDER_18 = 81;


        public const int FULL_PROVISIONAL_LICENSE_UNDER_21 = 82;


        public const int GENEVA_CONVENTIONS_IDENTITY_CARD = 83;


        public const int GRADUATED_DRIVING_LICENSE_UNDER_18 = 84;


        public const int GRADUATED_DRIVING_LICENSE_UNDER_21 = 85;


        public const int GRADUATED_INSTRUCTION_PERMIT_UNDER_18 = 86;


        public const int GRADUATED_INSTRUCTION_PERMIT_UNDER_21 = 87;


        public const int GRADUATED_LICENSE_UNDER_18 = 88;


        public const int GRADUATED_LICENSE_UNDER_21 = 89;


        public const int HANDGUN_CARRY_PERMIT = 90;


        public const int IDENTITY_AND_PRIVILEGE_CARD = 91;


        public const int IDENTITY_CARD_MOBILITY_IMPAIRED = 92;


        public const int IDENTITY_CARD_REGISTERED_OFFENDER = 93;


        public const int IDENTITY_CARD_TEMPORARY_VISITOR = 94;


        public const int IDENTITY_CARD_TEMPORARY_VISITOR_UNDER_18 = 95;


        public const int IDENTITY_CARD_TEMPORARY_VISITOR_UNDER_21 = 96;


        public const int IDENTITY_CARD_UNDER_18 = 97;


        public const int IDENTITY_CARD_UNDER_21 = 98;


        public const int IGNITION_INTERLOCK_PERMIT = 100;


        public const int IMMIGRANT_VISA = 101;


        public const int INSTRUCTION_PERMIT = 102;


        public const int INSTRUCTION_PERMIT_UNDER_18 = 103;


        public const int INSTRUCTION_PERMIT_UNDER_21 = 104;


        public const int INTERIM_DRIVING_LICENSE = 105;


        public const int INTERIM_IDENTITY_CARD = 106;


        public const int INTERMEDIATE_DRIVING_LICENSE = 107;


        public const int INTERMEDIATE_DRIVING_LICENSE_UNDER_18 = 108;


        public const int INTERMEDIATE_DRIVING_LICENSE_UNDER_21 = 109;


        public const int JUNIOR_DRIVING_LICENSE = 110;


        public const int LEARNER_INSTRUCTIONAL_PERMIT = 111;


        public const int LEARNER_LICENSE = 112;


        public const int LEARNER_LICENSE_UNDER_18 = 113;


        public const int LEARNER_LICENSE_UNDER_21 = 114;


        public const int LEARNER_PERMIT = 115;


        public const int LEARNER_PERMIT_UNDER_18 = 116;


        public const int LEARNER_PERMIT_UNDER_21 = 117;


        public const int LIMITED_LICENSE = 118;


        public const int LIMITED_PERMIT = 119;


        public const int LIMITED_TERM_DRIVING_LICENSE = 120;


        public const int LIMITED_TERM_IDENTITY_CARD = 121;


        public const int LIQUOR_IDENTITY_CARD = 122;


        public const int NEW_PERMIT = 123;


        public const int NEW_PERMIT_UNDER_18 = 124;


        public const int NEW_PERMIT_UNDER_21 = 125;


        public const int NON_US_CITIZEN_DRIVING_LICENSE = 126;


        public const int OCCUPATIONAL_DRIVING_LICENSE = 127;


        public const int ONEIDA_TRIBE_OF_INDIANS_IDENTITY_CARD = 128;


        public const int OPERATOR_LICENSE = 129;


        public const int OPERATOR_LICENSE_UNDER_18 = 130;


        public const int OPERATOR_LICENSE_UNDER_21 = 131;


        public const int PERMANENT_DRIVING_LICENSE = 132;


        public const int PERMIT_TO_REENTER = 133;


        public const int PROBATIONARY_AUTO_LICENSE = 134;


        public const int PROBATIONARY_DRIVING_LICENSE_UNDER_18 = 135;


        public const int PROBATIONARY_DRIVING_LICENSE_UNDER_21 = 136;


        public const int PROBATIONARY_VEHICLE_SALES_PERSON_LICENSE = 137;


        public const int PROVISIONAL_DRIVING_LICENSE = 138;


        public const int PROVISIONAL_DRIVING_LICENSE_UNDER_18 = 139;


        public const int PROVISIONAL_DRIVING_LICENSE_UNDER_21 = 140;


        public const int PROVISIONAL_LICENSE = 141;


        public const int PROVISIONAL_LICENSE_UNDER_18 = 142;


        public const int PROVISIONAL_LICENSE_UNDER_21 = 143;


        public const int PUBLIC_PASSENGER_CHAUFFEUR_LICENSE = 144;


        public const int RACING_AND_GAMING_COMISSION_CARD = 145;


        public const int REFUGEE_TRAVEL_DOCUMENT = 146;


        public const int RENEWAL_PERMIT = 147;


        public const int RESTRICTED_COMMERCIAL_DRIVER_LICENSE = 148;


        public const int RESTRICTED_DRIVER_LICENSE = 149;


        public const int RESTRICTED_PERMIT = 150;


        public const int SEASONAL_PERMIT = 151;


        public const int SEASONAL_RESIDENT_IDENTITY_CARD = 152;


        public const int SEASONAL_CITIZEN_IDENTITY_CARD = 153;


        public const int SEX_OFFENDER = 154;


        public const int SOCIAL_SECURITY_CARD = 155;


        public const int TEMPORARY_DRIVING_LICENSE = 156;


        public const int TEMPORARY_DRIVING_LICENSE_UNDER_18 = 157;


        public const int TEMPORARY_DRIVING_LICENSE_UNDER_21 = 158;


        public const int TEMPORARY_IDENTITY_CARD = 159;


        public const int TEMPORARY_INSTRUCTION_PERMIT_IDENTITY_CARD = 160;


        public const int TEMPORARY_INSTRUCTION_PERMIT_IDENTITY_CARD_UNDER_18 = 161;


        public const int TEMPORARY_INSTRUCTION_PERMIT_IDENTITY_CARD_UNDER_21 = 162;


        public const int TEMPORARY_VISITOR_DRIVING_LICENSE = 163;


        public const int TEMPORARY_VISITOR_DRIVING_LICENSE_UNDER_18 = 164;


        public const int TEMPORARY_VISITOR_DRIVING_LICENSE_UNDER_21 = 165;


        public const int UNIFORMED_SERVICES_IDENTITY_CARD = 166;


        public const int VEHICLE_SALES_PERSON_LICENSE = 167;


        public const int WORKER_IDENTIFICATION_CREDENTIAL = 168;


        public const int COMMERCIAL_DRIVING_LICENSE_NOVICE = 169;


        public const int COMMERCIAL_DRIVING_LICENSE_NOVICE_UNDER_18 = 170;


        public const int COMMERCIAL_DRIVING_LICENSE_NOVICE_UNDER_21 = 171;


        public const int PASSPORT_CARD = 172;


        public const int PASSPORT_RESIDENT_CARD = 173;


        public const int PERSONAL_IDENTIFICATION_VERIFICATION = 174;


        public const int TEMPORARY_OPERATOR_LICENSE = 175;


        public const int DRIVING_LICENSE_UNDER_19 = 176;


        public const int IDENTITY_CARD_UNDER_19 = 177;


        public const int VISA = 178;


        public const int TEMPORARY_PASSPORT = 179;


        public const int VOTING_CARD = 180;


        public const int HEALTH_CARD = 181;


        public const int CERTIFICATE_OF_CITIZENSHIP = 182;


        public const int ADDRESS_CARD = 183;


        public const int AIRPORT_IMMIGRATION_CARD = 184;


        public const int ALIEN_REGISTRATION_CARD = 185;


        public const int APEH_CARD = 186;


        public const int COUPON_TO_DRIVING_LICENSE = 187;


        public const int CREW_MEMBER_CERTIFICATE = 188;


        public const int DOCUMENT_FOR_RETURN = 189;


        public const int E_CARD = 190;


        public const int EMPLOYMENT_CARD = 191;


        public const int HKSAR_IMMIGRATION_FORM = 192;


        public const int IMMIGRANT_CARD = 193;


        public const int LABOUR_CARD = 194;


        public const int LAISSEZ_PASSER = 195;


        public const int LAWYER_IDENTITY_CERTIFICATE = 196;


        public const int LICENSE_CARD = 197;


        public const int PASSPORT_STATELESS = 198;


        public const int PASSPORT_CHILD = 199;


        public const int PASSPORT_CONSULAR = 200;


        public const int PASSPORT_DIPLOMATIC_SERVICE = 201;


        public const int PASSPORT_OFFICIAL = 202;


        public const int PASSPORT_PROVISIONAL = 203;


        public const int PASSPORT_SPECIAL = 204;


        public const int PERMISSION_TO_THE_LOCAL_BORDER_TRAFFIC = 205;


        public const int REGISTRATION_CERTIFICATE = 206;


        public const int SEDESOL_CARD = 207;


        public const int SOCIAL_CARD = 208;


        public const int TB_CARD = 209;


        public const int VEHICLE_PASSPORT = 210;


        public const int W_DOCUMENT = 211;


        public const int DIPLOMATIC_IDENTITY_CARD = 212;


        public const int CONSULAR_IDENTITY_CARD = 213;


        public const int INCOME_TAX_CARD = 214;


        public const int RESIDENCE_PERMIT = 215;


        public const int DOCUMENT_OF_IDENTITY = 216;


        public const int BORDER_CROSSING_PERMIT = 217;


        public const int PASSPORT_LIMITED_VALIDITY = 218;


        public const int SIM_CARD = 219;


        public const int TAX_CARD = 220;


        public const int COMPANY_CARD = 221;


        public const int DOMESTIC_PASSPORT = 222;


        public const int IDENTITY_CERTIFICATE = 223;


        public const int RESIDENT_ID_CARD = 224;


        public const int ARMED_FORCES_IDENTITY_CARD = 225;


        public const int PROFESSIONAL_CARD = 226;


        public const int REGISTRATION_STAMP = 227;


        public const int DRIVER_CARD = 228;


        public const int DRIVER_TRAINING_CERTIFICATE = 229;


        public const int QUALIFICATION_DRIVING_LICENSE = 230;


        public const int MEMBERSHIP_CARD = 231;


        public const int PUBLIC_VEHICLE_DRIVER_AUTHORITY_CARD = 232;


        public const int MARINE_LICENSE = 233;


        public const int TEMPORARY_LEARNER_LICENSE = 234;


        public const int TEMPORARY_COMMERCIAL_DRIVING_LICENSE = 235;


        public const int INTERIM_INSTRUCTIONAL_PERMIT = 236;


        public const int CERTIFICATE_OF_COMPETENCY = 237;


        public const int CERTIFICATE_OF_PROFICIENCY = 238;


        public const int TRADE_LICENSE = 239;


        public const int PASSPORT_PAGE = 240;

    }
    public class UserData
    {
        public string UserId { get; set; }
        public string Password { get; set; }
    }

    public enum eVisualFieldType
    {
        ft_Document_Class_Code = 0,
        ft_Issuing_State_Code = 1,
        ft_Document_Number = 2,
        ft_Date_of_Expiry = 3,
        ft_Date_of_Issue = 4,
        ft_Date_of_Birth = 5,
        ft_Place_of_Birth = 6,
        ft_Personal_Number = 7,
        ft_Surname = 8,
        ft_Given_Names = 9,
        ft_Mothers_Name = 10,
        ft_Nationality = 11,
        ft_Sex = 12,
        ft_Height = 13,
        ft_Weight = 14,
        ft_Eyes_Color = 15,
        ft_Hair_Color = 16,
        ft_Address = 17,
        ft_Donor = 18,
        ft_Social_Security_Number = 19,
        ft_DL_Class = 20,
        ft_DL_Endorsed = 21,
        ft_DL_Restriction_Code = 22,
        ft_DL_Under_21_Date = 23,
        ft_Authority = 24,
        ft_Surname_And_Given_Names = 25,
        ft_Nationality_Code = 26,
        ft_Passport_Number = 27,
        ft_Invitation_Number = 28,
        ft_Visa_ID = 29,
        ft_Visa_Class = 30,
        ft_Visa_SubClass = 31,
        ft_MRZ_String1 = 32,
        ft_MRZ_String2 = 33,
        ft_MRZ_String3 = 34,
        ft_MRZ_Type = 35,
        ft_Optional_Data = 36,
        ft_Document_Class_Name = 37,
        ft_Issuing_State_Name = 38,
        ft_Place_of_Issue = 39,
        ft_Document_Number_Checksum = 40,
        ft_Date_of_Birth_Checksum = 41,
        ft_Date_of_Expiry_Checksum = 42,
        ft_Personal_Number_Checksum = 43,
        ft_FinalChecksum = 44,
        ft_Passport_Number_Checksum = 45,
        ft_Invitation_Number_Checksum = 46,
        ft_Visa_ID_Checksum = 47,
        ft_Surname_And_Given_Names_Checksum = 48,
        ft_Visa_Valid_Until_Checksum = 49,
        ft_Other = 50,
        ft_MRZ_Strings = 51,
        ft_Name_Suffix = 52,
        ft_Name_Prefix = 53,
        ft_Date_of_Issue_Checksum = 54,
        ft_Date_of_Issue_CheckDigit = 55,
        ft_Document_Series = 56,
        ft_RegCert_RegNumber = 57,
        ft_RegCert_CarModel = 58,
        ft_RegCert_CarColor = 59,
        ft_RegCert_BodyNumber = 60,
        ft_RegCert_CarType = 61,
        ft_RegCert_MaxWeight = 62,
        ft_Reg_Cert_Weight = 63,
        ft_Address_Area = 64,
        ft_Address_State = 65,
        ft_Address_Building = 66,
        ft_Address_House = 67,
        ft_Address_Flat = 68,
        ft_Place_of_Registration = 69,
        ft_Date_of_Registration = 70,
        ft_Resident_From = 71,
        ft_Resident_Until = 72,
        ft_Authority_Code = 73,
        ft_Place_of_Birth_Area = 74,
        ft_Place_of_Birth_StateCode = 75,
        ft_Address_Street = 76,
        ft_Address_City = 77,
        ft_Address_Jurisdiction_Code = 78,
        ft_Address_Postal_Code = 79,
        ft_Document_Number_CheckDigit = 80,
        ft_Date_of_Birth_CheckDigit = 81,
        ft_Date_of_Expiry_CheckDigit = 82,
        ft_Personal_Number_CheckDigit = 83,
        ft_FinalCheckDigit = 84,
        ft_Passport_Number_CheckDigit = 85,
        ft_Invitation_Number_CheckDigit = 86,
        ft_Visa_ID_CheckDigit = 87,
        ft_Surname_And_Given_Names_CheckDigit = 88,
        ft_Visa_Valid_Until_CheckDigit = 89,
        ft_Permit_DL_Class = 90,
        ft_Permit_Date_of_Expiry = 91,
        ft_Permit_Identifier = 92,
        ft_Permit_Date_of_Issue = 93,
        ft_Permit_Restriction_Code = 94,
        ft_Permit_Endorsed = 95,
        ft_Issue_Timestamp = 96,
        ft_Number_of_Duplicates = 97,
        ft_Medical_Indicator_Codes = 98,
        ft_Non_Resident_Indicator = 99,
        ft_Visa_Type = 100,
        ft_Visa_Valid_From = 101,
        ft_Visa_Valid_Until = 102,
        ft_Duration_of_Stay = 103,
        ft_Number_of_Entries = 104,
        ft_Day = 105,
        ft_Month = 106,
        ft_Year = 107,
        ft_Unique_Customer_Identifier = 108,
        ft_Commercial_Vehicle_Codes = 109,
        ft_AKA_Date_of_Birth = 110,
        ft_AKA_Social_Security_Number = 111,
        ft_AKA_Surname = 112,
        ft_AKA_Given_Names = 113,
        ft_AKA_Name_Suffix = 114,
        ft_AKA_Name_Prefix = 115,
        ft_Mailing_Address_Street = 116,
        ft_Mailing_Address_City = 117,
        ft_Mailing_Address_Jurisdiction_Code = 118,
        ft_Mailing_Address_Postal_Code = 119,
        ft_Audit_Information = 120,
        ft_Inventory_Number = 121,
        ft_Race_Ethnicity = 122,
        ft_Jurisdiction_Vehicle_Class = 123,
        ft_Jurisdiction_Endorsement_Code = 124,
        ft_Jurisdiction_Restriction_Code = 125,
        ft_Family_Name = 126,
        ft_Given_Names_RUS = 127,
        ft_Visa_ID_RUS = 128,
        ft_Fathers_Name = 129,
        ft_Fathers_Name_RUS = 130,
        ft_Surname_And_Given_Names_RUS = 131,
        ft_Place_Of_Birth_RUS = 132,
        ft_Authority_RUS = 133,
        ft_Issuing_State_Code_Numeric = 134,
        ft_Nationality_Code_Numeric = 135,
        ft_Engine_Power = 136,
        ft_Engine_Volume = 137,
        ft_Chassis_Number = 138,
        ft_Engine_Number = 139,
        ft_Engine_Model = 140,
        ft_Vehicle_Category = 141,
        ft_Identity_Card_Number = 142,
        ft_Control_No = 143,
        ft_Parrent_s_Given_Names = 144,
        ft_Second_Surname = 145,
        ft_Middle_Name = 146,
        ft_RegCert_VIN = 147,
        ft_RegCert_VIN_CheckDigit = 148,
        ft_RegCert_VIN_Checksum = 149,
        ft_Line1_CheckDigit = 150,
        ft_Line2_CheckDigit = 151,
        ft_Line3_CheckDigit = 152,
        ft_Line1_Checksum = 153,
        ft_Line2_Checksum = 154,
        ft_Line3_Checksum = 155,
        ft_RegCert_RegNumber_CheckDigit = 156,
        ft_RegCert_RegNumber_Checksum = 157,
        ft_RegCert_Vehicle_ITS_Code = 158,
        ft_Card_Access_Number = 159,
        ft_Marital_Status = 160,
        ft_Company_Name = 161,
        ft_Special_Notes = 162,
        ft_Surname_of_Spose = 163,
        ft_Tracking_Number = 164,
        ft_Booklet_Number = 165,
        ft_Children = 166,
        ft_Copy = 167,
        ft_Serial_Number = 168,
        ft_Dossier_Number = 169,
        ft_AKA_Surname_And_Given_Names = 170,
        ft_Territorial_Validity = 171,
        ft_MRZ_Strings_With_Correct_CheckSums = 172,
        ft_DL_CDL_Restriction_Code = 173,
        ft_DL_Under_18_Date = 174,
        ft_DL_Record_Created = 175,
        ft_DL_Duplicate_Date = 176,
        ft_DL_Iss_Type = 177,
        ft_Military_Book_Number = 178,
        ft_Destination = 179,
        ft_Blood_Group = 180,
        ft_Sequence_Number = 181,
        ft_RegCert_BodyType = 182,
        ft_RegCert_CarMark = 183,
        ft_Transaction_Number = 184,
        ft_Age = 185,
        ft_Folio_Number = 186,
        ft_Voter_Key = 187,
        ft_Address_Municipality = 188,
        ft_Address_Location = 189,
        ft_Section = 190,
        ft_OCR_Number = 191,
        ft_Federal_Elections = 192,
        ft_Reference_Number = 193,
        ft_Optional_Data_Checksum = 194,
        ft_Optional_Data_CheckDigit = 195,
        ft_Visa_Number = 196,
        ft_Visa_Number_Checksum = 197,
        ft_Visa_Number_CheckDigit = 198,
        ft_Voter = 199,
        ft_Previous_Type = 200,
        ft_FieldFromMRZ = 220,
        ft_CurrentDate = 221,
        ft_Status_Date_of_Expiry = 251,
        ft_Banknote_Number = 252,
        ft_CSC_Code = 253,
        ft_Artistic_Name = 254,
        ft_Academic_Title = 255,
        ft_Address_Country = 256,
        ft_Address_Zipcode = 257,
        ft_eID_Residence_Permit1 = 258,
        ft_eID_Residence_Permit2 = 259,
        ft_eID_PlaceOfBirth_Street = 260,
        ft_eID_PlaceOfBirth_City = 261,
        ft_eID_PlaceOfBirth_State = 262,
        ft_eID_PlaceOfBirth_Country = 263,
        ft_eID_PlaceOfBirth_Zipcode = 264,
        ft_CDL_Class = 265,
        ft_DL_Under_19_Date = 266,
        ft_Weight_pounds = 267,
        ft_Limited_Duration_Document_Indicator = 268,
        ft_Endorsement_Expiration_Date = 269,
        ft_Revision_Date = 270,
        ft_Compliance_Type = 271,
        ft_Family_name_truncation = 272,
        ft_First_name_truncation = 273,
        ft_Middle_name_truncation = 274,
        ft_Exam_Date = 275,
        ft_Organization = 276,
        ft_Department = 277,
        ft_Pay_Grade = 278,
        ft_Rank = 279,
        ft_Benefits_Number = 280,
        ft_Sponsor_Service = 281,
        ft_Sponsor_Status = 282,
        ft_Sponsor = 283,
        ft_Relationship = 284,
        ft_USCIS = 285,
        ft_Category = 286,
        ft_Conditions = 287,
        ft_Identifier = 288,
        ft_Configuration = 289,
        ft_Discretionary_data = 290,
        ft_Line1_Optional_Data = 291,
        ft_Line2_Optional_Data = 292,
        ft_Line3_Optional_Data = 293,
        ft_EQV_Code = 294,
        ft_ALT_Code = 295,
        ft_Binary_Code = 296,
        ft_Pseudo_Code = 297,
        ft_Fee = 298,
        ft_Stamp_Number = 299,
        ft_GNIB_Number = 340,
        ft_Dept_Number = 341,
        ft_Telex_Code = 342,
        ft_Allergies = 343,
        ft_Sp_Code = 344,
        ft_Court_Code = 345,
        ft_Cty = 346,
        ft_Sponsor_SSN = 347,
        ft_DoD_Number = 348,
        ft_MC_Novice_Date = 349,
        ft_DUF_Number = 350,
        ft_AGY = 351,
        ft_PNR_Code = 352,
        ft_From_Airport_Code = 353,
        ft_To_Airport_Code = 354,
        ft_Flight_Number = 355,
        ft_Date_of_Flight = 356,
        ft_Seat_Number = 357,
        ft_Date_of_Issue_Boarding_Pass = 358,
        ft_CCW_Until = 359,
        ft_Reference_Number_Checksum = 360,
        ft_Reference_Number_CheckDigit = 361,
        ft_Room_Number = 362,
        ft_Religion = 363,
        ft_RemainderTerm = 364,
        ft_Electronic_Ticket_Indicator = 365,
        ft_Compartment_Code = 366,
        ft_CheckIn_Sequence_Number = 367,
        ft_Airline_Designator_of_boarding_pass_issuer = 368,
        ft_Airline_Numeric_Code = 369,
        ft_Ticket_Number = 370,
        ft_Frequent_Flyer_Airline_Designator = 371,
        ft_Frequent_Flyer_Number = 372,
        ft_Free_Baggage_Allowance = 373,
        ft_PDF417Codec = 374,
        ft_Identity_Card_Number_Checksum = 375,
        ft_Identity_Card_Number_CheckDigit = 376,
        ft_Veteran = 377,
        ft_DLClassCode_A1_From = 378,
        ft_DLClassCode_A1_To = 379,
        ft_DLClassCode_A1_Notes = 380,
        ft_DLClassCode_A_From = 381,
        ft_DLClassCode_A_To = 382,
        ft_DLClassCode_A_Notes = 383,
        ft_DLClassCode_B_From = 384,
        ft_DLClassCode_B_To = 385,
        ft_DLClassCode_B_Notes = 386,
        ft_DLClassCode_C1_From = 387,
        ft_DLClassCode_C1_To = 388,
        ft_DLClassCode_C1_Notes = 389,
        ft_DLClassCode_C_From = 390,
        ft_DLClassCode_C_To = 391,
        ft_DLClassCode_C_Notes = 392,
        ft_DLClassCode_D1_From = 393,
        ft_DLClassCode_D1_To = 394,
        ft_DLClassCode_D1_Notes = 395,
        ft_DLClassCode_D_From = 396,
        ft_DLClassCode_D_To = 397,
        ft_DLClassCode_D_Notes = 398,
        ft_DLClassCode_BE_From = 399,
        ft_DLClassCode_BE_To = 400,
        ft_DLClassCode_BE_Notes = 401,
        ft_DLClassCode_C1E_From = 402,
        ft_DLClassCode_C1E_To = 403,
        ft_DLClassCode_C1E_Notes = 404,
        ft_DLClassCode_CE_From = 405,
        ft_DLClassCode_CE_To = 406,
        ft_DLClassCode_CE_Notes = 407,
        ft_DLClassCode_D1E_From = 408,
        ft_DLClassCode_D1E_To = 409,
        ft_DLClassCode_D1E_Notes = 410,
        ft_DLClassCode_DE_From = 411,
        ft_DLClassCode_DE_To = 412,
        ft_DLClassCode_DE_Notes = 413,
        ft_DLClassCode_M_From = 414,
        ft_DLClassCode_M_To = 415,
        ft_DLClassCode_M_Notes = 416,
        ft_DLClassCode_L_From = 417,
        ft_DLClassCode_L_To = 418,
        ft_DLClassCode_L_Notes = 419,
        ft_DLClassCode_T_From = 420,
        ft_DLClassCode_T_To = 421,
        ft_DLClassCode_T_Notes = 422,
        ft_DLClassCode_AM_From = 423,
        ft_DLClassCode_AM_To = 424,
        ft_DLClassCode_AM_Notes = 425,
        ft_DLClassCode_A2_From = 426,
        ft_DLClassCode_A2_To = 427,
        ft_DLClassCode_A2_Notes = 428,
        ft_DLClassCode_B1_From = 429,
        ft_DLClassCode_B1_To = 430,
        ft_DLClassCode_B1_Notes = 431,
        ft_Surname_at_Birth = 432,
        ft_Civil_Status = 433,
        ft_Number_of_Seats = 434,
        ft_Number_of_Standing_Places = 435,
        ft_Max_Speed = 436,
        ft_Fuel_Type = 437,
        ft_EC_Environmental_Type = 438,
        ft_Power_Weight_Ratio = 439,
        ft_Max_Mass_of_Trailer_Braked = 440,
        ft_Max_Mass_of_Trailer_Unbraked = 441,
        ft_Transmission_Type = 442,
        ft_Trailer_Hitch = 443,
        ft_Accompanied_by = 444,
        ft_Police_District = 445,
        ft_First_Issue_Date = 446,
        ft_Payload_Capacity = 447,
        ft_Number_of_Axels = 448,
        ft_Permissible_Axle_Load = 449,
        ft_Precinct = 450,
        ft_Invited_by = 451,
        ft_Purpose_of_Entry = 452,
        ft_Skin_Color = 453,
        ft_Complexion = 454,
        ft_Airport_From = 455,
        ft_Airport_To = 456,
        ft_Airline_Name = 457,
        ft_Airline_Name_Frequent_Flyer = 458,
        ft_In_Tanks = 460,
        ft_Exept_In_Tanks = 461,
        ft_Fast_Track = 462,
        ft_Owner = 463,
        ft_MRZ_Strings_ICAO_RFID = 464,
    }


    public class LoginResponse
    {
        public string Token { get; set; }
        public bool IsLoggedIn { get; set; }
    }

    public class Picture
    {
        public string Base64ImageString { get; set; }
        public string Format { get; set; }
        public int LightIndex { get; set; } = 6;
        public int PageIndex { get; set; } = 0;
    }

    public class ReadDocumentModel
    {
        public List<Picture> Picture { get; set; }
    }


    public class ReadDocumentResponseModel
    {
        public bool result { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string gender { get; set; }
        public string dateOfBirth { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string issueCountry { get; set; }
        public string issueCountry_code2 { get; set; }
        public string issueCountry_fullname { get; set; }
        public string nationality { get; set; }
        public string nationality_code2 { get; set; }
        public string nationality_fullname { get; set; }
        public string documentNumber { get; set; }
        public string personalNumber { get; set; }
        public string issueDate { get; set; }
        public string expiryDate { get; set; }
        public string optionalData1 { get; set; }
        public string optionalData2 { get; set; }
        public string personalEyeColor { get; set; }
        public string personalHeight { get; set; }
        public string issuingPlace { get; set; }
        public string placeOfBirth { get; set; }
        public string countryOfBirth { get; set; }
        public string idType { get; set; }
        public string errorMessage { get; set; }
        public string fullImage { get; set; }
        public string faceImage { get; set; }
        public string errorCode { get; set; }
        public string cardNumber { get; set; }
        public string visaNumber { get; set; }
        public string arabicName { get; set; }
        public string mobileNumber { get; set; }
        public string fathersName { get; set; }
        public string mothersName { get; set; }
        public string registeredCity { get; set; }
        public string registeredTown { get; set; }
        public string fullName { get; set; }
        public string arabicDocumentNumber { get; set; }

        public string arabicNationality { get; set; }
        public string idCardType { get; set; }
        //public string martialStatus { get; set; }
        public string occupation { get; set; }

        public string CompanyNameArabic { get; set; }
        public string FieldofStudyArabic { get; set; }
        public string FieldofStudyEnglish { get; set; }
        public string PassportIssueCountryDescriptionArabic { get; set; }
        public string QualificationLevelDescriptionArabic { get; set; }
        public string CompanyNameEnglish { get; set; }
        public string OccupationField { get; set; }
        public string PassportIssueCountry { get; set; }
        public string PassportIssueCountryDescriptionEnglish { get; set; }
        public string PassportExpiryDate { get; set; }
        public string PassportIssueDate { get; set; }
        public string PassportNumber { get; set; }
        public string PassportType { get; set; }
        public string QualificationLevel { get; set; }
        public string QualificationLevelDescriptionEnglish { get; set; }
        public string ResidencyExpiryDate { get; set; }
        public string ResidencyNumber { get; set; }
        public string ResidencyType { get; set; }
        public string SponsorNumber { get; set; }
        public string SponsorType { get; set; }
        public string HomeAreaCode { get; set; }
        public string HomeAddressTypeCode { get; set; }
        public string HomeAreaDescriptionArabic { get; set; }
        public string HomeAreaDescriptionEnglish { get; set; }
        public string HomeCityCode { get; set; }
        public string HomeCityDescriptionArabic { get; set; }
        public string HomeCityDescriptionEnglish { get; set; }
        public string EmirateCode { get; set; }
        public string EmirateDescriptionArabic { get; set; }
        public string POBox { get; set; }
        public string HomeStreetArabic { get; set; }
        public string HomeStreetEnglish { get; set; }
        public string EmirateDescriptionEnglish { get; set; }
        public string spouseName { get; set; }
        public string martialStatus = "SINGLE";

        public string documentTypeAbrevation { get; set; }
        public string genderAbrevation { get; set; }


        //public string error_code { get; set; }
        //public string error_message { get; set; }

        public string DocumentResult { get; set; }
        public string fullImageIR { get; set; }
        public string[] AuthenticationAlerts { get; set; }
        public string fullImageUV { get; set; }
        public bool IsExpired { get; set; }
        public int age { get; set; }
        public string TransactionID { get; set; }
    }

    public class RegulaDocumentImageModel
    {
        public string Base64ImageString { get; set; }
        public string Format { get; set; }
        public int LightIndex { get; set; }
        public int PageIndex { get; set; }
    }

    public partial class DocumentType
    {
        public string documentTypeDescription { get; set; }
        public string documentTypeCode { get; set; }
        public string issueCountryCode { get; set; }
    }


    public partial class Document
    {
        [JsonProperty("result_type")]
        public long ResultType { get; set; }

        [JsonProperty("light")]
        public long Light { get; set; }

        [JsonProperty("buf_length")]
        public long BufLength { get; set; }

        [JsonProperty("list_idx")]
        public long ListIdx { get; set; }

        [JsonProperty("page_idx")]
        public long PageIdx { get; set; }

        [JsonProperty("DocGraphicsInfo")]
        public DocGraphicsInfo DocGraphicsInfo { get; set; }

        [JsonProperty("OneCandidate")]
        public OneCandidate OneCandidate { get; set; }

        [JsonProperty("Info")]
        public Info Info { get; set; }
    }

    public partial class OneCandidate
    {
        [JsonProperty("DocumentName")]
        public string DocumentName { get; set; }

        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("P")]
        public double P { get; set; }

        [JsonProperty("Rotated180")]
        public long Rotated180 { get; set; }

        [JsonProperty("RotationAngle")]
        public long RotationAngle { get; set; }

        [JsonProperty("NecessaryLights")]
        public long NecessaryLights { get; set; }

        [JsonProperty("RFID_Presence")]
        public long RfidPresence { get; set; }

        [JsonProperty("CheckAuthenticity")]
        public long CheckAuthenticity { get; set; }

        [JsonProperty("UVExp")]
        public long UvExp { get; set; }

        [JsonProperty("OVIExp")]
        public long OviExp { get; set; }

        [JsonProperty("AuthenticityNecessaryLights")]
        public long AuthenticityNecessaryLights { get; set; }

        [JsonProperty("FDSIDList")]
        public FdsidList FdsidList { get; set; }
    }

    public partial class FdsidList
    {
        [JsonProperty("ICAOCode")]
        public string IcaoCode { get; set; }

        [JsonProperty("Count")]
        public long Count { get; set; }

        [JsonProperty("List")]
        public object[] List { get; set; }

        [JsonProperty("dType")]
        public long DType { get; set; }

        [JsonProperty("dFormat")]
        public long DFormat { get; set; }

        [JsonProperty("dMRZ")]
        public bool DMrz { get; set; }

        [JsonProperty("dDescription")]
        public string DDescription { get; set; }

        [JsonProperty("dYear")]
        public string DYear { get; set; }

        [JsonProperty("dCountryName")]
        public string DCountryName { get; set; }
    }

    public partial class DocGraphicsInfo
    {
        [JsonProperty("nFields")]
        public long NFields { get; set; }

        [JsonProperty("pArrayFields")]
        public List<PArrayField> PArrayFields { get; set; }
    }

    public partial class PArrayField
    {
        [JsonProperty("FieldType")]
        public long FieldType { get; set; }

        [JsonProperty("FieldRect")]
        public FieldRect FieldRect { get; set; }

        [JsonProperty("FieldName")]
        public string FieldName { get; set; }

        [JsonProperty("image")]
        public Image Image { get; set; }
    }

    public partial class FieldRect
    {
        [JsonProperty("bottom")]
        public long Bottom { get; set; }

        [JsonProperty("left")]
        public long Left { get; set; }

        [JsonProperty("right")]
        public long Right { get; set; }

        [JsonProperty("top")]
        public long Top { get; set; }
    }

    public partial class Image
    {
        [JsonProperty("image")]
        public string ImageImage { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }
    }

    public partial class Info
    {
        [JsonProperty("DateTime")]
        public DateTimeOffset DateTime { get; set; }

        [JsonProperty("TransactionID")]
        public Guid TransactionId { get; set; }

        [JsonProperty("ComputerName")]
        public string ComputerName { get; set; }

        [JsonProperty("UserName")]
        public string UserName { get; set; }

        [JsonProperty("SDKVersion")]
        public string SdkVersion { get; set; }

        [JsonProperty("FileVersion")]
        public string FileVersion { get; set; }

        [JsonProperty("DeviceType")]
        public string DeviceType { get; set; }

        [JsonProperty("DeviceNumber")]
        public string DeviceNumber { get; set; }

        [JsonProperty("DeviceLabelNumber")]
        public string DeviceLabelNumber { get; set; }
    }

}