using System.Xml.Linq;

namespace SonosData.DataClasses
{
    public class AvailableServices
    {
        public AvailableServices() { }
        public AvailableServices(string Description, string _TypleList, string _Version)
        {
            TypeList = Array.ConvertAll(_TypleList.Split(','), int.Parse).ToList();
            AllServices = ParseServiceDescriptionList(Description);
            Version = Convert.ToInt16(_Version);
        }
        /// <summary>
        /// Interne Sonosversionierung
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// Liste mit Nummern. Unbekannt wofür. Scheinen nicht mit dem Service ID´s übereinzustimmen. 
        /// </summary>
        public List<int> TypeList { get; private set; } = new();
        /// <summary>
        /// Liste aller Services
        /// </summary>
        public List<AvailableService> AllServices { get; private set; } = new();

        /// <summary>
        /// Parst das übergebene XML zu einer Liste von Services
        /// </summary>
        /// <param name="des"></param>
        private static List<AvailableService> ParseServiceDescriptionList(string des)
        {
            try
            {
                var xml = XElement.Parse(des);

                var items = xml.Elements("Service");
                var list = new List<AvailableService>();

                foreach (var item in items)
                {
                    try
                    {
                        if (item == null) continue;
                        XElement policy = item.Element("Policy");
                        XElement presentation = item.Element("Presentation");
                        var ase = new AvailableService();
                        if (presentation != null)
                        {

                            XElement presentationstrings = presentation.Element("Strings");
                            XElement prenstationmap = presentation.Element("PresentationMap");
                            if (prenstationmap != null)
                            {
                                ase.Presentation.MapVersion = Convert.ToInt16((string)prenstationmap.Attribute("Version"));
                                ase.Presentation.MapURI = (string)prenstationmap.Attribute("Uri") ?? "";
                            }
                            if (presentationstrings != null)
                            {
                                ase.Presentation.Version = Convert.ToInt16((string)presentationstrings.Attribute("Version"));
                                ase.Presentation.URI = (string)presentationstrings.Attribute("Uri") ?? "";
                            }
                        }


                        if (policy != null && Enum.TryParse((string)policy.Attribute("Auth"), out SonosEnums.PolicyAuth pa))
                            ase.PolicyAuth = pa;
                        ase.ID = Convert.ToInt16((string)item.Attribute("Id"));
                        ase.Name = (string)item.Attribute("Name") ?? "";
                        ase.Version = (string)item.Attribute("Version") ?? "";
                        ase.URI = (string)item.Attribute("Uri") ?? "";
                        ase.SecureURI = (string)item.Attribute("SecureUri") ?? "";
                        ase.ContainerType = (string)item.Attribute("ContainerType") == "MService" ? SonosEnums.ContainerTypes.MService : SonosEnums.ContainerTypes.SoundLab;
                        if (policy != null)
                            ase.PollIntervall = Convert.ToInt16((string)policy.Attribute("PollInterval"));
                        ase.Presentation = new ServicePresentation();
                        list.Add(ase);
                    }
                    catch
                    {
                        continue;
                    }

                }
                return list;
            }
            catch
            {
                return new List<AvailableService>();
            }
        }
    }
    public class AvailableService
    {
        /// <summary>
        /// Interne ID des Service
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Name des Service
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// Version
        /// </summary>
        public string Version { get; set; } = "";
        /// <summary>
        /// URI
        /// </summary>
        public string URI { get; set; } = "";
        /// <summary>
        /// HTTPS URI
        /// </summary>
        public string SecureURI { get; set; } = "";
        /// <summary>
        /// Art des Container. Scheint Beta und Produktion zu unterscheiden
        /// </summary>
        public SonosEnums.ContainerTypes ContainerType { get; set; }
        /// <summary>
        /// Unbekannt.
        /// </summary>
        public int Capabilities { get; set; }

        /// <summary>
        /// Art der Authentifizierung
        /// </summary>
        public SonosEnums.PolicyAuth PolicyAuth { get; set; }
        /// <summary>
        /// Aufigkeit der Überprüfung der Authentifizierung
        /// </summary>
        public int PollIntervall { get; set; }
        /// <summary>
        /// Presentation Part des XML
        /// </summary>
        public ServicePresentation Presentation { get; set; } = new();
    }

    /// <summary>
    /// Presentation Part der XML
    /// </summary>
    public class ServicePresentation
    {
        /// <summary>
        /// Version des String Elements
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// URI des String Elements
        /// </summary>
        public string URI { get; set; } = "";
        /// <summary>
        /// Version des PresentationMap Elements
        /// </summary>
        public int MapVersion { get; set; }

        /// <summary>
        /// Uri des PresentationMap Elements
        /// </summary>
        public string MapURI { get; set; } = "";
    }
}
