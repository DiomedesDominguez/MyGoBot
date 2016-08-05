using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGB.WPF
{
    using Internals;

    using Newtonsoft.Json;

    internal class User
    {
        [JsonProperty(PropertyName = "account_type")]
        public AccountType AccountType
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "banned")]
        public int Banned
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "created_at")]
        public DateTime? CreatedAt
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "email")]
        public string Email
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "id")]
        public int Id
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "purchased")]
        public int Purchased
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "purchased_at")]
        public DateTime? PurchasedAt
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "trial")]
        public DateTime? Trial
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "updated_at")]
        public DateTime? UpdatedAt
        {
            get;
            set;
        }

        public User()
        {
        }
    }
}
