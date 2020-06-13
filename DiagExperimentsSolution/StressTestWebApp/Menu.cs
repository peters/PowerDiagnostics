﻿using CustomEventSource;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace StressTestWebApp
{
    public class Menu
    {
        private string _address;
        private int _concurrency;
        private ServiceProvider _serviceProvider;
        private string _postPayload;
        private List<MenuItem> _menuItems = new List<MenuItem>();

        public Menu(ServiceProvider serviceProvider, string address, int concurrency)
        {
            _serviceProvider = serviceProvider;
            _address = address;
            _concurrency = concurrency;

            // generate a random 1K string payload of readable characters
            Random rnd = new Random();
            _postPayload = new string(Enumerable.Range(0, 1024)
                .Select(n => (char)(rnd.Next(0, 96) + 32))
                .ToArray());

            _menuItems.Add(new MenuItem('1', HttpMethod.Get, "/Index", _concurrency));
            _menuItems.Add(new MenuItem('2', HttpMethod.Post, "/api/Test/SimplePost", _concurrency, true));
            _menuItems.Add(new MenuItem('3', HttpMethod.Post, "/api/Test/ExceptionOnPost", _concurrency));
            _menuItems.Add(new MenuItem('4', HttpMethod.Post, "/api/Test/SlowPost", _concurrency));
            _menuItems.Add(new MenuItem('5', HttpMethod.Post, "/api/Test/LeakBlob", 1));
            _menuItems.Add(new MenuItem('6', HttpMethod.Post, "/api/Test/LeakGraph", 1));
            _menuItems.Add(new MenuItem('7', HttpMethod.Post, "/api/Test/FreeLeaks", 1));
            _menuItems.Add(new MenuItem('8', HttpMethod.Post, "/api/Test/GcCollect", 1));
            _menuItems.Add(new MenuItem('9', HttpMethod.Post, "/api/Test/CpuStress", 1));
        }

        public async Task Start()
        {
            Usage(default(ConsoleKeyInfo));
            ConsoleKeyInfo keyInfo = default(ConsoleKeyInfo);
            do
            {
                bool ignore = false;
                keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.C:
                        break;

                    default:
                        ignore = true;
                        break;
                }

                var character = (char)((int)keyInfo.Key);
                var selectedMenu = _menuItems.FirstOrDefault(i => i.MenuKey == character);
                if (selectedMenu != null)
                {
                    if (selectedMenu.Verb == HttpMethod.Get)
                        await MakeGet(selectedMenu);
                    else if (selectedMenu.Verb == HttpMethod.Post)
                        await MakePost(selectedMenu);

                    ignore = false;
                }

                if (!ignore) Usage(keyInfo);
            }
            while (keyInfo.Key != ConsoleKey.Q);
        }

        private void Usage(ConsoleKeyInfo key)
        {
            Console.Clear();
            Console.WriteLine($"Last Command: {key.KeyChar}");
            Console.WriteLine($"Pid = {Process.GetCurrentProcess().Id}");
            foreach (var menuItem in _menuItems)
            {
                Console.WriteLine(menuItem.ToString());
            }
            Console.WriteLine($"C. Clear screen");
        }

        private async Task MakeGet(MenuItem menuItem)
        {
            Func<TestWebAppClient, Task<bool>> requestMaker = async client =>
            {
                if (!menuItem.UseCustomHeader)
                {
                    client.Client.DefaultRequestHeaders.Remove(Constants.TriggerHeaderName);
                }
                else
                {
                    client.Client.DefaultRequestHeaders.Add(Constants.TriggerHeaderName, "");
                }
                var result = await client.GetPage(menuItem.RelativeAddress);
                if (result)
                    Console.Write(".");
                else
                    Console.Write("X");
                return result;
            };

            await Execute(menuItem.Concurrency, requestMaker);
        }


        private async Task MakePost(MenuItem menuItem)
        {
            Func<TestWebAppClient, Task<bool>> requestMaker = async client =>
            {
                if (!menuItem.UseCustomHeader)
                {
                    client.Client.DefaultRequestHeaders.Remove(Constants.TriggerHeaderName);
                }
                else
                {
                    client.Client.DefaultRequestHeaders.Add(Constants.TriggerHeaderName, "");
                }
                var result = await client.Post(menuItem.RelativeAddress, _postPayload); 
                if (result)
                    Console.Write(".");
                else
                    Console.Write("X");
                return result;
            };

            await Execute(menuItem.Concurrency, requestMaker);
        }


        /// <summary>
        /// This method either call directly the func, or execute it in parallel
        /// using Task.Run
        /// Later it asks on the console when make the parallel call start together
        /// </summary>
        /// <param name="concurrency"></param>
        /// <param name="requestMaker"></param>
        /// <returns></returns>
        private async Task Execute(int concurrency, Func<TestWebAppClient, Task<bool>> requestMaker)
        {
            if (concurrency == 1)
            {
                var client = _serviceProvider.GetRequiredService<TestWebAppClient>();
                await requestMaker(client);
            }
            else
            {
                var evt = new ManualResetEventSlim();
                var requests = Enumerable.Range(0, concurrency)
                  .Select(_ =>
                  {
                      return Task.Run<bool>(() =>
                      {
                          var client = _serviceProvider.GetRequiredService<TestWebAppClient>();
                          evt.Wait();
                          return requestMaker(client);
                      });
                  })
                  .ToArray();

                Console.WriteLine("Requests ready to go. Press any key to run them");
                Console.ReadKey();
                Console.WriteLine("GO!");
                evt.Set();

                await Task.WhenAll(requests);
            }
        }

    }
}