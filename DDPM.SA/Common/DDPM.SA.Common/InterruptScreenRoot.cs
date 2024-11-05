using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public class InterruptScreenRoot
    {
        public int version { get; set; }
        public List<FeaturesList> featuresList { get; set; }
    }
    public class FeaturesList
    {
        public int categoryId { get; set; }
        public Content content { get; set; }
    }
    public class Content
    {
        public string id { get; set; }
        public string imageUrl { get; set; }
        public ProductLabel productLabel { get; set; }
        public List<DetailsList> detailsList { get; set; }
        public BugDescription bugDescription { get; set; }
    }
    public class ProductLabel
    {
        public string source { get; set; }
        public Translations translations { get; set; }
    }
    public class DetailsList
    {
        public string source { get; set; }
        public Translations translations { get; set; }
    }
    public class BugDescription
    {
        public string source { get; set; }
        public Translations translations { get; set; }
    }
    public class Translations
    {
        public string ar { get; set; }
        public string de { get; set; }
        public string es { get; set; }
        public string fr { get; set; }
        public string fr_CA { get; set; }
        public string it { get; set; }
        public string ja { get; set; }
        public string ko { get; set; }
        public string pl { get; set; }
        public string pt_BR { get; set; }
        public string ru { get; set; }
        public string tr { get; set; }
        public string uk { get; set; }
        public string zh { get; set; }
        public string zh_TW { get; set; }
    }
}
