using System.Collections.Generic;

namespace DDPM.SA.Obfuscation
{
    public class InfoHash
    {
        public static readonly List<string> Info_Hash = new List<string>()
        {
            "MIIBojANBgkqhkiG9w0BAQEFAAOCAY8AMIIBigKCAYEAr2Bev+kMcw+KhmJSQrMP9Qg7DgbgWd1a8OCfj4gYB3f2XkJ2ygckyC/SnAp6bvmm6OpaqtjZna96+xZsKefhrmLqWkt0P/dg6uD/NYxhObZrijp++8woep50ueXi6ktckcZOI0yhEIWebZinJwKeFQKVprXpw+7hwKfESR+X+aVf9WMJ56r35fhWW2B46WxZkUsGZMYquemfu9sHOHFUQwFpoTxb490X+lRweG6ryJNR3L9F/iIDjbvKnB6dVjFDAVfdRRWBEMAnxTeq+uvb3Suc2ehLFumL5nVZgNEGCAN3mWWcmQwl82PC028g5oxXoJ5FlRvyUm4hjRl5JmlLfiifycdm1MKN1EUFY05V8OGNT1fx59112IQ7U2kSlDscmPdz4nIZwSI1Y3kSiEUQV57xlhLu9mUkctDTjXhPyt91rdbm4/Lv2lj+HI9uU9FDbGtnrfQxSwlQK935TO8Ex+h+BIpvYuCbjxYzaRt6S88Af57bs3Fu9vRnodewbF8VAgMBAAE=", //3097 key length, Production key
            //[Dean] 20250120 starting from v66, remove Wistron testing key
            //"MIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAuDomNFsgnEKAzdOWVX2vQR8nG+ABP8XwWyXjDLr+Fb0SypiEOpI9F4uHBcKAeviJQMpVOYh5fWD/NQVwB+UvUbRJ7QXfcjmsEJuZ9yhQHbnQOermpt7UNyUCf5gkMMU1kMEOkxzz7VdJW6v8ZPy5ieNyGw4Pbt4isBi1j0RSSH/fSAGU+8G9iEsQDBd3U0pT5o+FphDOVTsfd0e3uP0xt6Tr1qsa0seGvHhFM9OXuovBg7RlzacmvQ2jlZZYkfdlejrbp+mp9Yu7OI1vNvhj656aeiQfwFTBxEWa1LYoVsUogSa2VpVBMaEs+ftsqcr92bkgb65qy8R1OZfDiBGL5I5zIjicFSpYrDnfAlZN2xhz3YE/LnOLDBlT718crGJaUYI0oQTdTKWcqGoSU4oVBu1NyZPmslxKoxoTNuZh/cc3lSH6joKDaXnU0uHnpRpLQgsDdfU7zFSxJ5FiJOcQN7ak7DBVO0m4AMZ419iH1Nw1MtEBD4SLVQGuP8cj/7vz37paNvJ0GCUH8r+p09hLuw0xZt1SAxzbjpWK965rRo/m5HKR/Iav9pSWPkW1/RBqv1FT6U/E/UW+1D3cZtLlspPYQPab7LZrKhibEyPg/dWw9BQNQZPUA1mZYjqAgag7/l8NI5s+YbvH0YZV78txOGI8JNukWAM1zwXH58x5TCUCAwEAAQ==" //Wistron test public key
        };
    }
}
