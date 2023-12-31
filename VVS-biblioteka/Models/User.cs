﻿using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace VVS_biblioteka.Models


{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public UserType UserType { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public DateTime ExpirationDate { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            User other = (User)obj;

            return Id == other.Id
                && FirstName == other.FirstName
                && LastName == other.LastName
                && Email == other.Email
                && PasswordHash == other.PasswordHash
                && UserType==other.UserType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, FirstName, LastName, Email, PasswordHash,UserType);
        }
    }
}
