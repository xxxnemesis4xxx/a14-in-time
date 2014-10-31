﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InTime.Models;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace InTime.Models
{
    public static class RequeteSql
    {
        public const string connectionString = @"Data Source=EQUIPE-02\SQLEXPRESS;Initial Catalog=InTime;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False";

        public static SqlConnection ConnexionBD(SqlConnection con)
        {
            con = new SqlConnection(connectionString);
            con.Open();

            return con;
        }

        public static int RechercheID(SqlConnection con, string username)
        {
            string SqlrId = "SELECT * FROM UserProfile where UserName=@NomUtilisateur;";

            SqlCommand cmdId = new SqlCommand(SqlrId, con);
            List<SqlParameter> Parametres = new List<SqlParameter>
            {
                new SqlParameter("@NomUtilisateur",username)
            };

            if (Parametres != null)
            {
                cmdId.Parameters.AddRange(Parametres.ToArray<SqlParameter>());
            }
                return (Int32)cmdId.ExecuteScalar();
        }

        public static SqlDataReader Select (string Query, List<SqlParameter> Parametres)
        {
            SqlConnection con = null;
            try
            {
                con = RequeteSql.ConnexionBD(con);
                SqlCommand cmdQuery = new SqlCommand(Query, con);

                if (Parametres != null)
                {
                    cmdQuery.Parameters.AddRange(Parametres.ToArray<SqlParameter>());
                }
                SqlDataReader reader = cmdQuery.ExecuteReader();

                return reader;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static bool ExecuteQuery(string Query, List<SqlParameter> Parametres)
        {
            SqlConnection con = null;
            try
            {
                con = RequeteSql.ConnexionBD(con);
                SqlCommand cmd = new SqlCommand(Query, con);
                cmd.Parameters.AddRange(Parametres.ToArray<SqlParameter>());
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static string EnleverApostrophe(string texte)
        {
            string Text = "";
            char apostrophe = Convert.ToChar(39);
            char doubleApostrophe = Convert.ToChar(34);

            foreach (char ch in texte)
            {
                if (ch == apostrophe)
                {

                    Text += doubleApostrophe;
                }
                else
                {
                    Text += ch;
                }
            }

            return Text;
        }


        public static string RemettreApostrophe(string texte)
        {
            string Text = "";
            char apostrophe = Convert.ToChar(39);
            char doubleApostrophe = Convert.ToChar(34);

            foreach (char ch in texte)
            {
                if (ch == doubleApostrophe)
                {

                    Text += Convert.ToChar(apostrophe);
                }
                else
                {
                    Text += ch;
                }
            }

            return Text;
        }
    }
}

