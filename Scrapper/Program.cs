using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Scrapper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var url = "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2380057.m570.l1313&_nkw=xbox+one&_sacat=0";
            List<HtmlNode> productList = await GetProductList(url);

            List<HtmlNode> productListItems, mainDetails;
            FilterHtmlResponse(productList, out productListItems, out mainDetails);

            List<string> title = new List<string>();
            List<string> subTitle = new List<string>();
            List<string> price = new List<string>();
            List<string> shippingPrice = new List<string>();
            List<string> country = new List<string>();

            ScrapeDataASync(productListItems, mainDetails, title, subTitle, price, shippingPrice, country);

            for (int i = 0; i < productListItems.Count; i++)
            {
                Console.WriteLine("Product # " + i);
                Console.WriteLine($"Name:\t\t{title[i]}");
                Console.WriteLine($"Subtitle:\t{subTitle[i]}");
                Console.WriteLine($"Price:\t\t{price[i]}");
                Console.WriteLine($"Shipping Price: {shippingPrice[i]} {country[i]}\n");
                Console.WriteLine("*********************************************************************************************************\n");

            }

        }

        private static void FilterHtmlResponse(List<HtmlNode> productList, out List<HtmlNode> productListItems, out List<HtmlNode> mainDetails)
        {
            productListItems = productList[0].Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                .Contains("s-item__info clearfix")).ToList();
            mainDetails = productList[0].Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                .Contains("s-item__detail s-item__detail--primary")).ToList();
        }

        private static void ScrapeDataASync(List<HtmlNode> productListItems, List<HtmlNode> mainDetails, List<string> title, List<string> subTitle, List<string> price, List<string> shippingPrice, List<string> country)
        {
            Task task1 = Task.Run(() =>
            {
                GetTitleOfItems(productListItems, title);

            });
            Task task2 = Task.Run(() =>
            {
                GetSubtitleOfItems(productListItems, subTitle);

            });
            Task task3 = Task.Run(() =>
            {
                GetPriceOfItems(productListItems, price);

            });
            Task task4 = Task.Run(() =>
            {
                GetShippingPriceOfItems(productListItems, shippingPrice);

            });
            Task task5 = Task.Run(() =>
            {
                GetCountryOfItems(mainDetails, country);

            });
            task1.Wait();
            task2.Wait();
            task3.Wait();
            task4.Wait();
            task5.Wait();

        }

        private static async Task<List<HtmlNode>> GetProductList(string url)
        {
            List<HtmlNode> productList = new List<HtmlNode>();
            await Task.Run(async () =>
            {
                HttpClient client = new HttpClient();
                string response = "";
                try
                {

                    response = await client.GetStringAsync(url);
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex);
                }

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(response);

                var List =
                    htmlDocument.DocumentNode.Descendants("ul")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Equals("srp-results srp-list clearfix")).ToList();
                productList = List;

            });
            return productList;
        }

        private static void GetCountryOfItems(List<HtmlNode> mainDetails, List<string> country)
        {

            foreach (var item in mainDetails)
            {
                var value = (item.Descendants("span")
                      .Where(node => node.GetAttributeValue("class", "")
                      .Equals("s-item__location s-item__itemLocation")).ToList());
                if (value.Count != 0)
                    country.Add(value[0].InnerText);
            }
        }

        private static void GetShippingPriceOfItems(List<HtmlNode> productListItems, List<string> shippingPrice)
        {
            foreach (var item in productListItems)
            {
                var value = (item.Descendants("span")
                      .Where(node => node.GetAttributeValue("class", "")
                      .Equals("s-item__shipping s-item__logisticsCost")).ToList());
                shippingPrice.Add(value[0].InnerText);
            }
        }

        private static void GetPriceOfItems(List<HtmlNode> productListItems, List<string> price)
        {
            foreach (var item in productListItems)
            {
                var value = (item.Descendants("span")
                      .Where(node => node.GetAttributeValue("class", "")
                      .Equals("s-item__price")).ToList());
                price.Add(value[0].InnerText);
            }
        }

        private static void GetSubtitleOfItems(List<HtmlNode> productListItems, List<string> subTitle)
        {
            foreach (var item in productListItems)
            {
                var value = (item.Descendants("div")
                      .Where(node => node.GetAttributeValue("class", "")
                      .Equals("s-item__subtitle")).ToList());
                subTitle.Add(value[0].InnerText);
            }
        }

        private static void GetTitleOfItems(List<HtmlNode> productListItems, List<string> title)
        {
            foreach (var item in productListItems)
            {
                var value = (item.Descendants("h3")
                      .Where(node => node.GetAttributeValue("class", "")
                      .Equals("s-item__title")).ToList());
                title.Add(value[0].InnerText);
            }
        }


    }
}
