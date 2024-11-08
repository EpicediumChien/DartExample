using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DDPM.SA.Common
{
    public class InterruptScreenRoot
    {
        public int version { get; set; }
        public List<FeaturesList> featuresList { get; set; }
        public bool Equals(InterruptScreenRoot other)
        {
            bool same = false;
            if (other.featuresList != null)
            {
                if (this.featuresList.Count == other.featuresList.Count)
                {
                    for (int i = 0; i < this.featuresList.Count; i++)
                    {
                        same = this.featuresList[i].Equals(other.featuresList[i]);
                        if (!same)
                        {
                            break;
                        }
                    }
                }
            }
            return this.version == other.version && same;
        }
    }
    public class FeaturesList
    {
        public int categoryId { get; set; }
        public Content content { get; set; }
        public bool Equals(FeaturesList other)
        {
            return this.categoryId == other.categoryId &&
                this.content.Equals(other.content);
        }
    }
    public class Content
    {
        public string id { get; set; }
        public string imageUrl { get; set; }
        public byte[] image { get; set; }
        public ProductLabel productLabel { get; set; }
        public List<DetailsList> detailsList { get; set; }
        public BugDescription bugDescription { get; set; }
        public bool Equals(Content other)
        {
            bool same = false;
            if (this.productLabel != null)
            {
                same = this.productLabel.Equals(other.productLabel);
            }
            if (this.detailsList != null)
            {
                if (this.detailsList.Count == other.detailsList.Count)
                {
                    for (int i = 0; i < this.detailsList.Count; i++)
                    {
                        same = this.detailsList[i].Equals(other.detailsList[i]);
                        if (!same)
                        {
                            break;
                        }
                    }
                }
            }
            if (this.bugDescription != null)
            {
                same = this.bugDescription.Equals(other.bugDescription);
            }
            return this.id == other.id &&
                this.imageUrl == other.imageUrl &&
                 same;
        }
    }
    public class ProductLabel
    {
        public string source { get; set; }
        public Translations translations { get; set; }
        public bool Equals(ProductLabel other)
        {
            return this.source == other.source;
        }
    }
    public class DetailsList
    {
        public string source { get; set; }
        public Translations translations { get; set; }
        public bool Equals(DetailsList other)
        {
            return this.source == other.source;
        }
    }
    public class BugDescription
    {
        public string source { get; set; }
        public Translations translations { get; set; }
        public bool Equals(BugDescription other)
        {
            return this.source == other.source;
        }
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
