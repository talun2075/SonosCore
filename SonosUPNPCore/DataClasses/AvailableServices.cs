using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SonosUPnP.DataClasses
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
        public List<int> TypeList { get; private set; }
        /// <summary>
        /// Liste aller Services
        /// </summary>
        public List<AvailableService> AllServices { get; private set; }

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
                        XElement policy = item.Element("Policy");
                        XElement prenstation = item.Element("Presentation");
                        XElement presentationstrings = prenstation.Element("Strings");
                        XElement prenstationmap = prenstation.Element("PresentationMap");
                        var ase = new AvailableService();
                        if (Enum.TryParse<SonosEnums.PolicyAuth>(policy.Attribute("Auth").Value, out SonosEnums.PolicyAuth pa))
                            ase.PolicyAuth = pa;
                        
                        ase.ID = Convert.ToInt16(item.Attribute("Id").Value);
                        ase.Name = item.Attribute("Name").Value;
                        ase.Version = item.Attribute("Version").Value;
                        ase.URI = item.Attribute("Uri").Value;
                        ase.SecureURI = item.Attribute("SecureUri").Value;
                        ase.ContainerType = item.Attribute("ContainerType").Value == "MService" ? SonosEnums.ContainerTypes.MService : SonosEnums.ContainerTypes.SoundLab;
                        ase.PollIntervall = Convert.ToInt16(policy.Attribute("PollInterval").Value);
                        ase.Presentation = new ServicePresentation();
                        if (prenstationmap != null)
                        {
                            ase.Presentation.MapVersion = Convert.ToInt16(prenstationmap.Attribute("Version").Value);
                            ase.Presentation.MapURI = prenstationmap.Attribute("Uri").Value;
                        }
                        if(presentationstrings != null)
                        {
                            ase.Presentation.Version = Convert.ToInt16(presentationstrings.Attribute("Version").Value);
                            ase.Presentation.URI = presentationstrings.Attribute("Uri").Value;
                        }
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
        public String Name { get; set; }
        /// <summary>
        /// Version
        /// </summary>
        public String Version { get; set; }
        /// <summary>
        /// URI
        /// </summary>
        public String URI { get; set; }
        /// <summary>
        /// HTTPS URI
        /// </summary>
        public String SecureURI { get; set; }
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
        public ServicePresentation Presentation { get; set; }
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
        public String URI { get; set; }
        /// <summary>
        /// Version des PresentationMap Elements
        /// </summary>
        public int MapVersion { get; set; }

        /// <summary>
        /// Uri des PresentationMap Elements
        /// </summary>
        public String MapURI { get; set; }
    }
}
