// 
// Message.cs
// Created by ilian000 on 2014-06-30
// Licenced under the Apache License, Version 2.0
//
      
namespace ClientCommon.Data
{
    public class Message
    {
        public Message()
        {
            shutdown = false;
        }
        public string title { get; set; }
        public string message { get; set; }
        public bool shutdown { get; set; }
    }
}
