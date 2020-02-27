﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gip.Models.Exceptions;
using System.Text.RegularExpressions;

namespace Gip.Models
{
    public partial class Room
    {


        // public string Gebouw { get; set; } //if(value.toLower.trim().length <= 0) throw new exeption, else gebouw = value
        // public int Verdiep { get; set; }
        // public string Nummer { get; set; }
        // public string Type { get; set; }
        // public int Capaciteit { get; set; }
        public virtual ICollection<CourseMoment> CourseMoment { get; set; }

        private string middelen;
        public string Middelen
        {
            get
            {
                return middelen;
            }
            set
            {
                string middelen1 = value.ToLower();
                int aantal = 1;
                if (middelen1.Contains('/'))
                {
                    aantal = middelen1.Split('/').Length;
                }
                if (aantal > 3)
                {
                    throw new DatabaseException("The amount of resources for a single room is to high!" + Environment.NewLine + "Please try again!");
                }
                int gevonden = 0;
                if (!middelen1.Trim().Equals(""))
                {
                    if (middelen1.Contains("projectorsetup"))
                    {
                        gevonden++;
                    }
                    if (middelen1.Contains("schermen"))
                    {
                        gevonden++;
                    }
                    else if (middelen1.Contains("scherm"))
                    {
                        gevonden++;
                    }
                    if (middelen1.Contains("wifi"))
                    {
                        gevonden++;
                    }
                    if (middelen1.Contains("geen middelen")) {
                        gevonden++;
                    }
                    if (gevonden != aantal)
                    {
                        throw new DatabaseException("The resources you selected are not available!" + Environment.NewLine + "Please try again!");
                    }
                    else
                    {
                        middelen = value.Trim();
                    }
                }
                else
                {
                    middelen = "Geen middelen";
                }

            }
        } 

        private string gebouw;
        public string Gebouw 
        {
            get { return gebouw; }

            set 
            {
                if (value.Trim().Length <= 0 || value.Trim().Length > 1)
                {
                    
                    throw new DatabaseException("The building you wish selected does not exist!" + Environment.NewLine + "Please try again.");
                }
                else
                {
                    string pattern = @"^[a-zA-Z]$";
                    if (Regex.IsMatch(value, pattern))
                    {
                        gebouw = value;
                    }
                    else
                    {
                        throw new DatabaseException("The building you wish selected does not exist!" + Environment.NewLine + "Please do not include any special characters and try again.");
                    }
                }
            }
        }

        private int verdiep;
        public int Verdiep
        {
            get { return verdiep; }
            set
            {
                if (value < 0 || value > 9)
                {
                    throw new DatabaseException("The floor you wish to select does not exist!" + Environment.NewLine + "Please try again.");
                }
                else
                {
                    string pattern = @"^\d$";
                    if (Regex.IsMatch(value.ToString(), pattern))
                    {
                        verdiep = value;
                    }
                    else
                    {
                        throw new DatabaseException("The floor you wish to select does not exist!" + Environment.NewLine + "Please do not include any special characters and try again.");
                    }
                }
            }
        }

        private string nummer;
        public string Nummer
        {
            get { return nummer; }
            set
            {
                if (value.Trim().Length > 3 || value.Trim().Length < 0)
                {
                    throw new DatabaseException("The number you wish to select is invalid!" + Environment.NewLine + "Please try again.");
                }
                else
                {
                    string pattern = @"^\d\d[a-zA-Z]{0,1}$";
                    if (Regex.IsMatch(value, pattern))
                    {
                        nummer = value;
                    }
                    else
                    {
                        throw new DatabaseException("The number you wish to select is invalid!" + Environment.NewLine + "Please do not include any special characters and try again.");
                    }
                }
            }
        }

        private string type;
        public string Type //is dropdown
        {
            get { return type; }
            set
            {
                if (value == "Computerlokaal" || value == "Aula" || value == "Vergaderlokaal" || value == "Lokaal")
                {
                    type = value;
                }
                else
                {
                    throw new DatabaseException("the type of room you wish to select does not exist!" + Environment.NewLine + "Please try again.");
                }
            }
        }

        private int capaciteit;

        public int Capaciteit   
        {
            get { return capaciteit; }
            set 
            {
                if (value > 400)
                {
                    throw new DatabaseException("The chosen capacity was to high!" + Environment.NewLine + "Please try again.");
                }
                else if (value < 0)
                {
                    throw new DatabaseException("The chosen capacity was to low!" + Environment.NewLine + "Please try again.");
                }
                else
                {
                    string pattern = @"^\d$";
                    if (Regex.IsMatch(value.ToString(), pattern))
                    {
                        capaciteit = value;
                    }
                    else
                    {
                        throw new DatabaseException("The capacity you wish to select does not exist!" + Environment.NewLine + "Please do not include any special characters and try again.");
                    }
                }
            }
        }

        public Room(string middelen, string gebouw, int verdiep, string nummer, string type, int capaciteit)
        {
            this.Capaciteit = capaciteit;
            this.Gebouw = gebouw;
            this.Verdiep = verdiep;
            this.Nummer = nummer;
            this.Middelen = middelen;
            this.Type = type;
        }
        public Room()
        {
            CourseMoment = new HashSet<CourseMoment>();
        }

    }
}
