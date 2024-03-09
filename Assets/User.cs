using System;
using JetBrains.Annotations;

namespace DefaultNamespace
{
    public class User
    {

        public User(string name)
        {
            lastSeen = DateTime.Now;
            Name = name;
        }

        public DateTime lastSeen;
        public string Name;

    }
}