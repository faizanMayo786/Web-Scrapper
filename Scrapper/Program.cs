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
            // Website Url to Fetch data
            string url = "https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2380057.m570.l1313&_nkw=xbox+one&_sacat=0";
            // Getting list if product list asyncronusly
            List<HtmlNode> productList = await GetProductList(url);
            // List of items and list of sub-details of items created
            List<HtmlNode> productListItems, mainDetails;
            // Seprating items & item-details from productlist
            FilterHtmlResponse(productList, out productListItems, out mainDetails);
            // Data containers/variables to store details of items
            List<string> title = new List<string>();
            List<string> subTitle = new List<string>();
            List<string> price = new List<string>();
            List<string> shippingPrice = new List<string>();
            List<string> country = new List<string>();
            // Scrapping details 
            ScrapeDataASync(productListItems, mainDetails, title, subTitle, price, shippingPrice, country);
            // Outputting details
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
            // seprating items form product list
            productListItems = productList[0].Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                .Contains("s-item__info clearfix")).ToList();
            // seperating main details of items from list
            mainDetails = productList[0].Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                .Contains("s-item__detail s-item__detail--primary")).ToList();
        }

        private static void ScrapeDataASync(List<HtmlNode> productListItems, List<HtmlNode> mainDetails, List<string> title, List<string> subTitle, List<string> price, List<string> shippingPrice, List<string> country)
        {
            // creating thread1 to fetch Title of product
            Task task1 = Task.Run(() =>
            {
                GetTitleOfItems(productListItems, title);

            });
            // creating thread2 to fetch Sub-Title of product
            Task task2 = Task.Run(() =>
            {
                GetSubtitleOfItems(productListItems, subTitle);

            });
            // creating thread3 to fetch Price of product
            Task task3 = Task.Run(() =>
            {
                GetPriceOfItems(productListItems, price);

            });
            // creating thread4 to fetch ShippingPrice of product
            Task task4 = Task.Run(() =>
            {
                GetShippingPriceOfItems(productListItems, shippingPrice);

            });
            // creating thread5 to fetch Country of product
            Task task5 = Task.Run(() =>
            {
                GetCountryOfItems(mainDetails, country);

            });

            // Waiting for all the thread/tasks to complete
            task1.Wait();
            task2.Wait();
            task3.Wait();
            task4.Wait();
            task5.Wait();

        }

        private static async Task<List<HtmlNode>> GetProductList(string url)
        {
            // temporary list
            List<HtmlNode> productList = new List<HtmlNode>();
            // Creating new thread and wait for thread to complete its process
            await Task.Run(async () =>
            {
                // http client to take our request
                HttpClient client = new HttpClient();
                // creating string response
                string response = "";

                try
                {
                    // waiting for html file as string from client
                    response = await client.GetStringAsync(url);
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex);
                }
                // creating HTML DOcument
                var htmlDocument = new HtmlDocument();
                // COnverting response String into HTMlDocument
                htmlDocument.LoadHtml(response);
                // Converting htmlDocument into ItemsList Document
                var List =
                    htmlDocument.DocumentNode.Descendants("ul")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Equals("srp-results srp-list clearfix")).ToList();
                productList = List;

            });
            // returning product list
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
                // seprate title from itemDetails
                var value = (item.Descendants("h3")
                      .Where(node => node.GetAttributeValue("class", "")
                      .Equals("s-item__title")).ToList());
                // add the title into List of title 
                title.Add(value[0].InnerText);
            }
        }


    }
}