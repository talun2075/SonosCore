namespace SonosData.DataClasses
{
    public static class SonosTimeZone
    {
        #region ListTimeZone
        private static readonly IList<SonosTimeZoneData> stz = new List<SonosTimeZoneData>
        { new SonosTimeZoneData { ID =0,ExternalString="(GMT-12:00) Internationale Datumsgrenze (Westen)",InternalString="02d000000000000000000000ffc4"},
            new SonosTimeZoneData {ID =1,ExternalString="(GMT-11:00) Midway-Inseln, Samoa",InternalString="029400000000000000000000ffc4"},
            new SonosTimeZoneData {ID =2,ExternalString="(GMT-10:00) Hawaii",InternalString="025800000000000000000000ffc4"},
            new SonosTimeZoneData {ID =3,ExternalString="(GMT-09:00) Alaska",InternalString="021c0b000102000003000202ffc4"},
            new SonosTimeZoneData {ID =4,ExternalString="(GMT-08:00) Pazific Zeit (USA und Kanada)",InternalString="01e00b000102000003000202ffc4"},
            new SonosTimeZoneData {ID =5,ExternalString="(GMT-07:00) Arizona",InternalString="01a400000000000000000000ffc4"},
            new SonosTimeZoneData {ID =6,ExternalString="(GMT-07:00) Chihuahua, La Paz, Mazatlan",InternalString="01a40a000502000004000102ffc4"},
            new SonosTimeZoneData {ID =7,ExternalString="(GMT-07:00) Mountain Time (Usa und Kanada)",InternalString="01a40b000102000003000202ffc4"},
            new SonosTimeZoneData {ID =8,ExternalString="(GMT-06:00) Zentralamerika",InternalString="016800000000000000000000ffc4"},
            new SonosTimeZoneData {ID =9,ExternalString="(GMT-06:00) Central Time (USA und Kanada)",InternalString="01680b000102000003000202ffc4"},
            new SonosTimeZoneData {ID =10,ExternalString="(GMT-06:00) Guadalajara, Mexico City, Monterrey",InternalString="01680a000502000004000102ffc4"},
            new SonosTimeZoneData {ID =11,ExternalString="(GMT-06:00) Saskatchewan",InternalString="016800000000000000000000ffc4"},
            new SonosTimeZoneData {ID =12,ExternalString="(GMT-05:00) Bogota, Lima, Quito",InternalString="012c00000000000000000000ffc4"},
            new SonosTimeZoneData {ID =13,ExternalString="(GMT-05:00) Eastern Time (USA und Kanada)",InternalString="012c0b000102000003000202ffc4"},
            new SonosTimeZoneData {ID =14,ExternalString="(GMT-05:00) Indiana (Osten)",InternalString="012c0b000102000003000202ffc4"},
            new SonosTimeZoneData {ID =15,ExternalString="(GMT-04:00) Atlantik Zeit (Kanada)",InternalString="00f00b000102000003000202ffc4"},
            new SonosTimeZoneData {ID =16,ExternalString="(GMT-04:00) Caracas, La Paz",InternalString="00f000000000000000000000ffc4"},
            new SonosTimeZoneData {ID =17,ExternalString="(GMT-04:00) Santiago",InternalString="00f00306020000000a060200ffc4"},
            new SonosTimeZoneData {ID =18,ExternalString="(GMT-03:00) Neufundland",InternalString="00d20b000102000003000202ffc4"},
            new SonosTimeZoneData {ID =19,ExternalString="(GMT-03:00) Brasilien",InternalString="00b40200020200000a000302ffc4"},
            new SonosTimeZoneData {ID =20,ExternalString="(GMT-03:00) Buenos Aires, Georgetown",InternalString="00b400000000000000000000ffc4"},
            new SonosTimeZoneData {ID =21,ExternalString="(GMT-03:00) Grönland",InternalString="00b40a000502000003000501ffc4"},
            new SonosTimeZoneData {ID =22,ExternalString="(GMT-02:00) Mittelatlantik",InternalString="007809000502000003000502ffc4"},
            new SonosTimeZoneData {ID =23,ExternalString="(GMT-01:00) Azoren",InternalString="003c0a000503000003000502ffc4"},
            new SonosTimeZoneData {ID =24,ExternalString="(GMT-01:00) Kapverdische Inseln",InternalString="003c00000000000000000000ffc4"},
            new SonosTimeZoneData {ID =25,ExternalString="(GMT) Casablanca, Monrovia",InternalString="000000000000000000000000ffc4"},
            new SonosTimeZoneData {ID =26,ExternalString="(GMT) Greewich Mean Time: Dublin, London",InternalString="00000a000502000003000501ffc4"},
            new SonosTimeZoneData {ID =27,ExternalString="(GMT+01:00) Amsterdam, Berlin, Bern, Rom",InternalString="ffc40a000503000003000502ffc4"},
            new SonosTimeZoneData {ID =28,ExternalString="(GMT+01:00) Belgrad, Bratislava, Budapest",InternalString="ffc40a000503000003000502ffc4"},
            new SonosTimeZoneData {ID =29,ExternalString="(GMT+01:00) Brüssel, Kopenhagen, Madrid, Paris",InternalString="ffc40a000503000003000502ffc4"},
            new SonosTimeZoneData {ID =30,ExternalString="(GMT+01:00) Sarajevo, Skopje, Warschau, Zagreb",InternalString="ffc40a000503000003000502ffc4"},
            new SonosTimeZoneData {ID =31,ExternalString="(GMT+01:00) Zentralafrika",InternalString="ffc400000000000000000000ffc4"},
            new SonosTimeZoneData {ID =32,ExternalString="(GMT+02:00) Athen",InternalString="ff880a000503000003000502ffc4"},
            new SonosTimeZoneData {ID =33,ExternalString="(GMT+02:00) Bukarest",InternalString="ff880a000501000003000500ffc4"},
            new SonosTimeZoneData {ID =34,ExternalString="(GMT+02:00) Kairo",InternalString="ff8809030502000005050102ffc4"},
            new SonosTimeZoneData {ID =35,ExternalString="(GMT+02:00) Harare, Prätoria",InternalString="ff8800000000000000000000ffc4"},
            new SonosTimeZoneData {ID =36,ExternalString="(GMT+02:00) Helsinki, Kiew, Riga, Sofia, Tallinn",InternalString="ff880a000504000003000503ffc4"},
            new SonosTimeZoneData {ID =37,ExternalString="(GMT+02:00) Jerusalem",InternalString="ff880a000502000003050502ffc4"},
            new SonosTimeZoneData {ID =38,ExternalString="(GMT+03:00) Bagdad, Istanbul, Minsk",InternalString="ff4c00000000000000000000ffc4"},
            new SonosTimeZoneData {ID =39,ExternalString="(GMT+03:00) Kuwait, Riad",InternalString="ff4c00000000000000000000ffc4"},
            new SonosTimeZoneData {ID =40,ExternalString="(GMT+03:00) Moskau, St. Petersburg, Wolgograd",InternalString="ff4c00000000000000000000ffc4"},
            new SonosTimeZoneData {ID =41,ExternalString="(GMT+03:00) Nairobi",InternalString="ff4c00000000000000000000ffc4"},
            new SonosTimeZoneData {ID =42,ExternalString="(GMT+03:30) Teheran",InternalString="ff2e09020402000003000102ffc4"},
            new SonosTimeZoneData {ID =43,ExternalString="(GMT+04:00) Abu Dhabi",InternalString="ff1000000000000000000000ffc4"},
            new SonosTimeZoneData {ID =44,ExternalString="(GMT+04:00) Baku, Tiflis",InternalString="ff100a000503000003000502ffc4"},
            new SonosTimeZoneData {ID =45,ExternalString="(GMT+04:30) Kabul",InternalString="fef200000000000000000000ffc4"},
            new SonosTimeZoneData {ID =46,ExternalString="(GMT+05:00) Jekaterinburg",InternalString="fed40a000503000003000502ffc4"},
            new SonosTimeZoneData {ID =47,ExternalString="(GMT+05:00) Islamabad, Karatschi, Taschkent",InternalString="fed400000000000000000000ffc4"},
            new SonosTimeZoneData {ID =48,ExternalString="(GMT+05:30) Chennai, Kolkata, Mumbai",InternalString="feb600000000000000000000ffc4"},
            new SonosTimeZoneData {ID =49,ExternalString="(GMT+05:45) Katmandu",InternalString="fea700000000000000000000ffc4"},
            new SonosTimeZoneData {ID =50,ExternalString="(GMT+06:00) Almaty, Nowosibirsk",InternalString="fe980a000503000003000502ffc4"},
            new SonosTimeZoneData {ID =51,ExternalString="(GMT+06:00) Astana, Dhaka",InternalString="fe9800000000000000000000ffc4"},
            new SonosTimeZoneData {ID =52,ExternalString="(GMT+06:00) Sri Jayawardenepura",InternalString="fe9800000000000000000000ffc4"},
            new SonosTimeZoneData {ID =53,ExternalString="(GMT+06:30) Rangu",InternalString="fe7a00000000000000000000ffc4"},
            new SonosTimeZoneData {ID =54,ExternalString="(GMT+07:00) Bankok, Hanoi, Jakarta",InternalString="fe5c00000000000000000000ffc4"},
            new SonosTimeZoneData {ID =55,ExternalString="(GMT+07:00) Krasnojarsk",InternalString="fe5c0a000503000003000502ffc4"},
            new SonosTimeZoneData {ID =56,ExternalString="(GMT+08:00) Peking, Chongqing, Hongkong",InternalString="fe2000000000000000000000ffc4"},
            new SonosTimeZoneData {ID =57,ExternalString="(GMT+08:00) Irkutsk, Ulan, Bator",InternalString="fe200a000503000003000502ffc4"},
            new SonosTimeZoneData {ID =58,ExternalString="(GMT+08:00) Kuala Lumpur, Singapur",InternalString="fe2000000000000000000000ffc4"},
            new SonosTimeZoneData {ID =59,ExternalString="(GMT+08:00) Perth",InternalString="fe2000000000000000000000ffc4"},
            new SonosTimeZoneData {ID =60,ExternalString="(GMT+08:00) Taipeh",InternalString="fe2000000000000000000000ffc4"},
            new SonosTimeZoneData {ID =61,ExternalString="(GMT+08:00) Osaka, Sapporo, Tokio",InternalString="fde400000000000000000000ffc4"},
            new SonosTimeZoneData {ID =62,ExternalString="(GMT+09:00) Seoul",InternalString="fde400000000000000000000ffc4"},
            new SonosTimeZoneData {ID =63,ExternalString="(GMT+09:00) Jakutsk",InternalString="fde40a000503000003000502ffc4"},
            new SonosTimeZoneData {ID =64,ExternalString="(GMT+09:30) Adelaide",InternalString="fdc60400010300000a000102ffc4"},
            new SonosTimeZoneData {ID =65,ExternalString="(GMT+09:30) Darwin",InternalString="fdc600000000000000000000ffc4"},
            new SonosTimeZoneData {ID =66,ExternalString="(GMT+10:00) Brisbane",InternalString="fda800000000000000000000ffc4"},
            new SonosTimeZoneData {ID =67,ExternalString="(GMT+10:00) Canberra, Melbourne, Sydney",InternalString="fda80400010300000a000102ffc4"},
            new SonosTimeZoneData {ID =68,ExternalString="(GMT+10:00) Guam, Port Moresby",InternalString="fda800000000000000000000ffc4"},
            new SonosTimeZoneData {ID =69,ExternalString="(GMT+10:00) Hobart",InternalString="fda80400010300000a000102ffc4"},
            new SonosTimeZoneData {ID =70,ExternalString="(GMT+10:00) Wladiwostok",InternalString="fda80a000503000003000502ffc4"},
            new SonosTimeZoneData {ID =71,ExternalString="(GMT+11:00) Magadan, Salomonen, Neukaledonien",InternalString="fd6c00000000000000000000ffc4"},
            new SonosTimeZoneData {ID =72,ExternalString="(GMT+12:00) Auckland, Wellington",InternalString="fd3004000103000009000502ffc4"},
            new SonosTimeZoneData {ID =73,ExternalString="(GMT+12:00) Fidschi, Kamatschatka, Marshallinseln",InternalString="fd3000000000000000000000ffc4"},
            new SonosTimeZoneData {ID =74,ExternalString="(GMT+13:00) Nuku'alofa",InternalString="fcf400000000000000000000ffc4"},
        };

        #endregion ListTimeZone
        /// <summary>
        /// Liefert die ID zum externen String
        /// </summary>
        /// <param name="externalString">999 = Nothing Found</param>
        /// <returns></returns>
        public static int GetIDByExternalString(string externalString)
        {
            SonosTimeZoneData id = stz.First(x => x.ExternalString == externalString);
            if (id == null)
            {
                return 999;
            }
            return id.ID;

        }
        /// <summary>
        /// Liefert die ID zum internen String
        /// </summary>
        /// <param name="externalString">999 = Nothing Found</param>
        /// <returns></returns>
        public static int GetIDByInternalString(string internalString)
        {
            SonosTimeZoneData id = stz.First(x => x.InternalString == internalString);
            if (id == null)
            {
                return 999;
            }
            return id.ID;

        }
        /// <summary>
        /// Liefert den externen String über die ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetExternalStringByID(int id)
        {
            SonosTimeZoneData es = stz.First(x => x.ID == id);
            if (es == null)
            {
                return string.Empty;
            }
            return es.ExternalString;
        }
        /// <summary>
        /// Liefert den internen String über die ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetInternalStringByID(int id)
        {
            SonosTimeZoneData es = stz.First(x => x.ID == id);
            if (es == null)
            {
                return string.Empty;
            }
            return es.InternalString;
        }
        /// <summary>
        /// Füllt die Leeren Werte vom Übergeben Objekt wenn möglich.
        /// </summary>
        /// <param name="stzd"></param>
        /// <returns></returns>
        public static SonosTimeZoneData? FillSonosTimeZoneData(SonosTimeZoneData stzd)
        {
            var retval = GetListOfTimeZones.FirstOrDefault(x => x.ID == stzd.ID);
            if (retval == null)
                retval = GetListOfTimeZones.FirstOrDefault(x => x.InternalString == stzd.InternalString);
            if (retval == null)
                retval = GetListOfTimeZones.FirstOrDefault(x => x.ExternalString == stzd.ExternalString);
            return retval;
        }

        public static IList<SonosTimeZoneData> GetListOfTimeZones => stz;
    }
}
