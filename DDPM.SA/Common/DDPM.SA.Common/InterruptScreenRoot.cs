using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DDPM.SA.Common
{
    public sealed class InterruptScreenRoot
    {
        public int version { get; set; }
        public List<FeaturesList> featuresList { get; set; }
        public InterruptScreenRoot()
        {
            version = 0;
            featuresList = new List<FeaturesList>();
        }
        public bool Equals(InterruptScreenRoot other)
        {
            bool same = false;
            if (other.featuresList != null && this.featuresList.Count == other.featuresList.Count)
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
            return this.version == other.version && same;
        }
        public InterruptScreenRoot Clone()
        {
            InterruptScreenRoot other = new InterruptScreenRoot();
            other.version = this.version;
            other.featuresList = this.featuresList;
            if (this.featuresList != null)
            {
                other.featuresList = new List<FeaturesList>();
                for (int i = 0; i < this.featuresList.Count; i++)
                {
                    other.featuresList.Add(this.featuresList[i].Clone());
                }
            }
            return other;
        }
    }
    public sealed class FeaturesList
    {
        public int categoryId { get; set; }
        public Content content { get; set; }
        public FeaturesList()
        {
            categoryId = 0;
            content = new Content();
        }
        public bool Equals(FeaturesList other)
        {
            return this.categoryId == other.categoryId &&
                this.content.Equals(other.content);
        }
        public FeaturesList Clone()
        {
            FeaturesList other = new FeaturesList();
            other.categoryId = this.categoryId;
            other.content = this.content;
            if (this.content != null)
            {
                other.content = this.content.Clone();
            }
            return other;
        }
    }
    public sealed class Content
    {
        public string id { get; set; }
        public string imageUrl { get; set; }
        public byte[] image { get; set; }
        public bool IsShowName { get; set; }
        public ProductLabel productLabel { get; set; }
        public List<DetailsList> detailsList { get; set; }
        public BugDescription bugDescription { get; set; }
        public Content()
        {
            id = string.Empty;
            imageUrl = string.Empty;
            image = new byte[0];
            IsShowName = true;
            productLabel = new ProductLabel();
            detailsList = new List<DetailsList>();
            bugDescription = new BugDescription();
        }
        public bool Equals(Content other)
        {
            bool same = false;
            if (this.productLabel != null)
            {
                same = this.productLabel.Equals(other.productLabel);
            }
            if (this.detailsList != null &&
                this.detailsList.Count == other.detailsList.Count)
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
            if (this.bugDescription != null)
            {
                same = this.bugDescription.Equals(other.bugDescription);
            }
            return this.id == other.id &&
                this.imageUrl == other.imageUrl &&
                 same;
        }
        public Content Clone()
        {
            Content other = new Content();
            other.id = this.id;
            other.imageUrl = this.imageUrl;
            other.image = this.image;
            other.productLabel = this.productLabel;
            other.bugDescription = this.bugDescription;
            other.detailsList = this.detailsList;
            return other;
        }
    }
    public sealed class ProductLabel
    {
        public string source { get; set; }
        public Translations translations { get; set; }
        public ProductLabel()
        {
            source = string.Empty;
            translations = new Translations();
        }
        public bool Equals(ProductLabel other)
        {
            return this.source == other.source;
        }
    }
    public class DetailsList
    {
        public string source { get; set; }
        public Translations translations { get; set; }
        public DetailsList()
        {
            source = string.Empty;
            translations = new Translations();
        }
        public bool Equals(DetailsList other)
        {
            return this.source == other.source;
        }
    }
    public sealed class BugDescription
    {
        public string source { get; set; }
        public Translations translations { get; set; }
        public BugDescription()
        {
            source = string.Empty;
            translations = new Translations();
        }
        public bool Equals(BugDescription other)
        {
            return this.source == other.source;
        }
    }
    public class Translations
    {
        public string ar { get; set; } = string.Empty;
        public string de { get; set; } = string.Empty;
        public string es { get; set; } = string.Empty;
        public string fr { get; set; } = string.Empty;
        public string fr_CA { get; set; } = string.Empty;
        public string it { get; set; } = string.Empty;
        public string ja { get; set; } = string.Empty;
        public string ko { get; set; } = string.Empty;
        public string pl { get; set; } = string.Empty;
        public string pt_BR { get; set; } = string.Empty;
        public string ru { get; set; } = string.Empty;
        public string tr { get; set; } = string.Empty;
        public string uk { get; set; } = string.Empty;
        public string zh { get; set; } = string.Empty;
        public string zh_TW { get; set; } = string.Empty;
    }
}
