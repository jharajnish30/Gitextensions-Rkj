﻿namespace Git.hub
{
    public class User
    {
        /// <summary>
        /// The GitHub username
        /// </summary>
        public string Login { get; internal set; }

        /// <summary>
        /// The avatar URL
        /// </summary>
        public string AvatarUrl { get; set; }

        public User()
        {
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode() + Login.GetHashCode();
        }

        public override string ToString()
        {
            return Login;
        }
    }
}

