﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Web.Mvc;
using InTime.Models;
using System.Globalization;
using System.Data.SqlClient;


namespace InTime.Controllers
{
    public class ConsulterTacheController : Controller
    {
        CultureInfo culture = new CultureInfo("fr-CA");

        public ActionResult Taches(string strMessValidation)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    try
                    {
                        var lstTache = new List<Tache>();
                        double DateAuj = TraitementDate.DateTimeToUnixTimestamp();
                        string queryString = "SELECT * FROM Taches where UserId=@Id AND (DateDebut>=@DateDebut OR Recurrence >= 0)";
                        List<SqlParameter> Parametres = new List<SqlParameter>
                    {
                        new SqlParameter("@Id",InTime.Models.Cookie.ObtenirCookie(User.Identity.Name)),
                        new SqlParameter("@DateDebut", DateAuj)
                    };


                        SqlDataReader reader = RequeteSql.Select(queryString, Parametres);
                        while (reader.Read())
                        {
                            Object[] values = new Object[reader.FieldCount];
                            reader.GetValues(values);
                            var tache = ObtenirTache(values);
                            DateTime DateTache = TraitementDate.UnixTimeStampToDateTime(tache.unixDebut);
                            tache.Annee = Convert.ToString(DateTache.Year);
                            tache.Mois = Convert.ToString(DateTache.Month);
                            tache.Jour = Convert.ToString(DateTache.Day);
                            lstTache.Add(tache);
                        }
                        reader.Close();
                        ViewBag.Taches = lstTache;

                        return View();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.ToString());
                    }


                }
                else
                {
                    return View(UrlErreur.Authentification);
                }
            }
            catch
            {
                return View(UrlErreur.Authentification);
            }
        }

        public ActionResult SuppTache(int? id)
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    string SqlDelete = "DELETE FROM Taches WHERE UserId=@UserId AND IdTache=@IdTache";
                    List<SqlParameter> Parametres = new List<SqlParameter>
                    {
                        new SqlParameter("@UserId",InTime.Models.Cookie.ObtenirCookie(User.Identity.Name)),
                        new SqlParameter("@IdTache",id)
                    };
                    RequeteSql.ExecuteQuery(SqlDelete, Parametres);

                    return RedirectToAction("Taches", "ConsulterTache");
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
            else
            {
                return View(UrlErreur.Authentification);
            }
        }

        public ActionResult ModifTache(int? id, double? dep, double? fn, bool? Existe)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                else
                    if (User.Identity.IsAuthenticated)
                    {
                        try
                        {
                            Tache tache = RechercherTache(id);

                            if (dep != null && fn != null)
                            {
                                tache.unixDebut = Convert.ToDouble(dep);
                                tache.unixFin = Convert.ToDouble(fn);
                                tache.Description = RechercheDescriptionTache(id, tache.unixDebut) ?? tache.Description;
                                ViewBag.Modif = true;
                            }
                            else
                            {
                                ViewBag.Modif = false;
                            }

                            PreparationPourAffichage(ref tache);
                            InitialiseViewBags();
                            Tache.InitChampsTache(ref tache);
                            ViewData["Tache"] = tache;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.ToString());
                        }

                        return View("Modification");
                    }
                    else
                    {
                        return View(UrlErreur.Authentification);
                    }
            }
            catch
            {
                return View(UrlErreur.Authentification);
            }
        }

        [HttpPost]
        public ActionResult Modification(Tache Model, string modif, bool Existe)
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    string SqlCommande;
                    List<SqlParameter> listParametres = new List<SqlParameter>();
                    int UserId = Int32.Parse(InTime.Models.Cookie.ObtenirCookie(User.Identity.Name));
                    double unixDebut = TraitementDate.DateTimeToUnixTimestamp(TraitementDate.DateDebut(Model));
                    double unixFin = TraitementDate.DateTimeToUnixTimestamp(TraitementDate.DateFin(Model));
                    string couleur = (Request.Form.GetValues(16).GetValue(0)).ToString();
                    Model.PriorityColor = couleur;
                    if (modif == "False")
                    {
                        SqlCommande = "UPDATE Taches set NomTache=@NomTache,Lieu=@Lieu,Description=@Description,"
                        + "DateDebut=@DateDebut,DateFin=@DateFin,HRappel=@HRappel,mRappel=@mRappel,"
                        + "recurrence=@recurrence, PriorityColor=@PriorityColor WHERE UserId=@UserId AND IdTache=@IdTache;";
                        listParametres.Add(new SqlParameter("@IdTache", Model.IdTache));
                        listParametres.Add(new SqlParameter("@UserId", UserId));
                        listParametres.Add(new SqlParameter("@NomTache", Model.NomTache));
                        listParametres.Add(new SqlParameter("@Lieu", Model.Lieu));
                        listParametres.Add(new SqlParameter("@DateDebut", unixDebut));
                        listParametres.Add(new SqlParameter("@DateFin", unixFin));
                        listParametres.Add(new SqlParameter("@Description", Model.Description));
                        listParametres.Add(new SqlParameter("@HRappel", SqlDbType.VarChar) { Value = Model.HRappel ?? (object)DBNull.Value });
                        listParametres.Add(new SqlParameter("@mRappel", SqlDbType.VarChar) { Value = Model.mRappel ?? (object)DBNull.Value });
                        listParametres.Add(new SqlParameter("@recurrence", Model.Recurrence));
                        listParametres.Add(new SqlParameter("@PriorityColor", Model.PriorityColor));
                    }
                    else
                    {
                        if (!Existe)
                        {
                            SqlCommande = "INSERT INTO InfoSupplTacheRecurrente (IdTache,DateDebut,DateFin,Description)"
                            + " VALUES (@IdTache,@DateDebut,@DateFin,@Description);";
                        }
                        else
                        {
                            SqlCommande = "UPDATE InfoSupplTacheRecurrente SET Description=@Description WHERE IdTache=@IdTache "
                                + "AND DateDebut=@DateDebut AND DateFin=@DateFin;";
                        }
                        listParametres.Add(new SqlParameter("@IdTache", Model.IdTache));
                        listParametres.Add(new SqlParameter("@DateDebut", unixDebut));
                        listParametres.Add(new SqlParameter("@DateFin", unixFin));
                        listParametres.Add(new SqlParameter("@Description", Model.Description));
                        listParametres.Add(new SqlParameter("@PriorityColor", Model.PriorityColor));
                    }
                    var message = RequeteSql.ExecuteQuery(SqlCommande, listParametres) ? "Modif" : "Echec";
                    TempData["Modification"] = message;

                    DateTime date = TraitementDate.UnixTimeStampToDateTime(unixDebut);

                    return RedirectToAction("Index", "Calendrier", new { @annee = date.Year, @mois = (date.Month - 1), @jour = date.Day });
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
            else
            {
                return View(UrlErreur.Authentification);
            }
        }

        public ActionResult Index(int? id, DateTime? dep, DateTime? fn, double deb = 0, double fin = 0)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                else
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        try
                        {
                            Tache tache = RechercherTache(id);

                            if (dep != null && fn != null)
                            {
                                tache.unixDebut = TraitementDate.DateTimeToUnixTimestamp(Convert.ToDateTime(dep));
                                tache.unixFin = TraitementDate.DateTimeToUnixTimestamp(Convert.ToDateTime(fn));
                                ViewBag.Modif = true;
                            }
                            else
                            {
                                if (deb > 0 && fin > 0)
                                {
                                    tache.unixDebut = deb;
                                    tache.unixFin = fin;
                                }
                                ViewBag.Modif = false;
                            }

                            string result = RechercheDescriptionTache(id, tache.unixDebut);
                            if (!String.IsNullOrEmpty(result))
                            {
                                ViewBag.Existe = true;
                                tache.Description = result;
                            }

                            PreparationPourAffichage(ref tache);
                            ViewData["Tache"] = tache;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.ToString());
                        }

                        return View();
                    }
                    else
                    {
                        return View(UrlErreur.Authentification);
                    }
                }
            }
            catch
            {
                return View(UrlErreur.Authentification);
            }
        }

        public ActionResult Historique(string ChoixTemps, string FinAnn, string DebAnn, string ChoixMoisFin, string ChoixMoisDebut, bool Recurrence = false)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    int Choix = 1;
                    int anneeDebut;
                    int anneeFin;
                    anneeDebut = anneeFin = DateTime.Now.Year;


                    if (!String.IsNullOrEmpty(ChoixTemps))
                    {
                        if (!Int32.TryParse(ChoixTemps, out Choix) || Choix == 0)
                        {
                            Choix = 1;
                        }
                        Int32.TryParse(FinAnn, out anneeFin);
                        Int32.TryParse(DebAnn, out anneeDebut);
                    }

                    ViewBag.Choix = Choix;
                    ViewBag.anneeDebut = anneeDebut;
                    ViewBag.anneeFin = anneeFin;
                    ViewBag.ChoixTemps = Tache.Choix_Historique;
                    ViewBag.ChoixMoisFin = Tache.les_mois;
                    ViewBag.ChoixMoisDebut = Tache.les_mois;
                    ViewBag.Taches = TraitementChoixHistorique(Choix, FinAnn, DebAnn, ChoixMoisFin, ChoixMoisDebut, Recurrence);

                    return View();
                }
                else
                {
                    return View(UrlErreur.Authentification);
                }
            }
            catch
            {
                return View(UrlErreur.Authentification);
            }
        }

        private List<Tache> TraitementChoixHistorique(int Choix, string FinAnn, string DebAnn, string ChoixMoisFin, string ChoixMoisDebut, bool Recurrence)
        {
            string Select = "";
            var lstTache = new List<Tache>();
            List<SqlParameter> listParametres = new List<SqlParameter>();
            double TacheRecDebut = 0;
            double TacheRecFin = 0;

            switch (Choix)
            {
                case 0:
                    break;
                case 1:
                    TacheRecDebut = TraitementDate.DateTimeToUnixTimestamp(DateTime.Now.AddMonths(-3));
                    TacheRecFin = TraitementDate.DateTimeToUnixTimestamp(DateTime.Now);
                    if (!Recurrence)
                    {
                        Select = "SELECT * FROM Taches where UserId=@Id AND DateDebut>@TroisMoisArriere AND DateDebut < @Maintenant AND Recurrence = 0;";
                    }
                    else
                    {
                        Select = "SELECT * FROM Taches where UserId=@Id AND ((DateDebut>@TroisMoisArriere AND DateDebut < @Maintenant AND Recurrence = 0) OR Recurrence > 0);";
                    }
                    listParametres.Add(new SqlParameter("@Id", InTime.Models.Cookie.ObtenirCookie(User.Identity.Name)));
                    listParametres.Add(new SqlParameter("@TroisMoisArriere", TacheRecDebut));
                    listParametres.Add(new SqlParameter("@Maintenant", TacheRecFin));
                    break;
                case 2:
                    try
                    {
                        DateTime Date1 = new DateTime(Convert.ToInt32(DebAnn), Convert.ToInt32(ChoixMoisDebut), 1);
                        DateTime Date2 = new DateTime(Convert.ToInt32(FinAnn), Convert.ToInt32(ChoixMoisFin), 1);
                        Date2 = Date2.AddMonths(1);
                        Date2 = Date2.AddDays(-1);
                        TacheRecDebut = TraitementDate.DateTimeToUnixTimestamp(Date1);
                        TacheRecFin = TraitementDate.DateTimeToUnixTimestamp(Date2);

                        if (!Recurrence)
                        {
                            Select = "SELECT * FROM Taches where UserId=@Id AND DateDebut >= @Date1 AND DateDebut <= @Date2 AND Recurrence = 0;";
                        }
                        else
                        {
                            Select = "SELECT * FROM Taches where UserId=@Id AND ((DateDebut >= @Date1 AND DateDebut <= @Date2 AND Recurrence = 0) OR Recurrence > 0);";
                        }
                        listParametres.Add(new SqlParameter("@Id", InTime.Models.Cookie.ObtenirCookie(User.Identity.Name)));
                        listParametres.Add(new SqlParameter("@Date1", TacheRecDebut));
                        listParametres.Add(new SqlParameter("@Date2", TacheRecFin));
                    }
                    catch (Exception ex)
                    {
                        Select = "";
                    }
                    break;
                case 3:
                    TacheRecDebut = TraitementDate.DebutCalendrier();
                    TacheRecFin = TraitementDate.DateTimeToUnixTimestamp();
                    if (!Recurrence)
                    {
                        Select = "SELECT * FROM Taches where UserId=@Id AND DateDebut < @Maintenant AND Recurrence = 0;";
                    }
                    else
                    {
                        Select = "SELECT * FROM Taches where UserId=@Id AND ((DateDebut < @Maintenant AND Recurrence = 0) OR Recurrence > 0);";
                    }
                    listParametres.Add(new SqlParameter("@Id", InTime.Models.Cookie.ObtenirCookie(User.Identity.Name)));
                    listParametres.Add(new SqlParameter("@Maintenant", TacheRecFin));
                    break;
                default:
                    break;
            }

            try
            {
                if (!String.IsNullOrEmpty(Select))
                {
                    SqlDataReader reader = RequeteSql.Select(Select, listParametres);
                    while (reader.Read())
                    {
                        Object[] values = new Object[reader.FieldCount];
                        reader.GetValues(values);
                        var tache = ObtenirTache(values);
                        if (tache.Recurrence == (int)TraitementDate.recurrence.Aucune)
                        {
                            DateTime DateTache = TraitementDate.UnixTimeStampToDateTime(tache.unixDebut);
                            tache.Annee = Convert.ToString(DateTache.Year);
                            tache.Mois = Convert.ToString(DateTache.Month);
                            tache.Jour = Convert.ToString(DateTache.Day);
                            lstTache.Add(tache);
                        }
                        else
                        {
                            List<Tache> result = TraitementDate.TraitementRecurrenceTache(tache, TacheRecDebut, TacheRecFin);
                            if (result != null)
                            {
                                lstTache.AddRange(result);
                            }
                        }
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                lstTache = new List<Tache>();
            }

            return lstTache;
        }

        private Tache RechercherTache(int? id)
        {
            string queryString = "SELECT * FROM Taches where IdTache=@Id";
            List<SqlParameter> Parametre = new List<SqlParameter>
                        {
                            new SqlParameter("@Id", id)
                        };

            SqlDataReader reader = RequeteSql.Select(queryString, Parametre);
            while (reader.Read())
            {
                Object[] values = new Object[reader.FieldCount];
                reader.GetValues(values);

                return ObtenirTache(values);
            }

            return null;
        }

        private String RechercheDescriptionTache(int? id, double Debut)
        {
            string queryString = "SELECT * FROM InfoSupplTacheRecurrente WHERE IdTache=@Id AND DateDebut=@Debut";
            List<SqlParameter> Parametres = new List<SqlParameter>
                            {
                                new SqlParameter("@Id", id),
                                new SqlParameter("@Debut", Debut)
                            };
            SqlDataReader reader = RequeteSql.Select(queryString, Parametres);
            while (reader.Read())
            {
                Object[] values = new Object[reader.FieldCount];
                reader.GetValues(values);

                return Convert.ToString(values[4]);
            }

            return null;
        }

        private string TempsRappel(DateTime rappel)
        {
            string strPhrase = "Il vous reste ";

            if (rappel < DateTime.Now)
            {
                return "La date de rappel est dépassée.";
            }
            else
            {
                TimeSpan tsTempsRestant = rappel - DateTime.Now;
                int nNombreJourRestant = tsTempsRestant.Days;
                if (nNombreJourRestant > 365)
                {
                    int nAnnee = (tsTempsRestant.Days / 365);
                    nNombreJourRestant -= (nAnnee * 365);
                    strPhrase += String.Format("{0} {1} ", nAnnee, nAnnee == 1 ? "an" : "ans");
                }

                if (nNombreJourRestant > 30)
                {
                    int nMois = (nNombreJourRestant / 30);
                    nNombreJourRestant -= (nMois * 30);
                    strPhrase += String.Format("{0} mois ", nMois);
                }

                if (nNombreJourRestant > 0)
                {
                    int nJours = nNombreJourRestant;
                    strPhrase += String.Format("{0} {1} ", nJours, nJours == 1 ? "jour" : "jours");
                }

                if (tsTempsRestant.Hours > 0)
                {
                    int nHeure = tsTempsRestant.Hours;
                    strPhrase += String.Format("{0} {1} ", nHeure, nHeure == 1 ? "heure" : "heures");
                }

                if (tsTempsRestant.Minutes > 0)
                {
                    int nMinute = tsTempsRestant.Minutes;
                    strPhrase += String.Format("{0} {1} ", nMinute, nMinute == 1 ? "minute" : "minutes");
                }

                return strPhrase + "avant le rappel.";
            }
        }

        private Tache ObtenirTache(Object[] values)
        {
            var tache = new Tache()
            {
                IdTache = Convert.ToInt32(values[0]),
                UserId = Convert.ToInt32(values[1]),
                NomTache = Convert.ToString(values[2]),
                Lieu = Convert.ToString(values[3]),
                Description = Convert.ToString(values[4]),
                unixDebut = Convert.ToDouble(values[5]),
                unixFin = Convert.ToDouble(values[6]),
                HRappel = Convert.ToString(values[7]),
                mRappel = Convert.ToString(values[8]),
                Recurrence = Convert.ToInt32(values[9]),
                PriorityColor = Convert.ToString(values[10])
            };

            return tache;
        }

        private void InitialiseViewBags()
        {
            ViewBag.trancheMin = new SelectList(Tache.tempsMinutes);

            ViewBag.trancheHeure = new SelectList(Tache.tempsHeure);

            ViewBag.MoisAnnee = new SelectList(Tache.les_mois, "Value", "Text");

            ViewBag.recurrence = new SelectList(Tache.options, "Value", "Text");
        }

        private void PreparationPourAffichage(ref Tache tache)
        {
            DateTime DateDebut = TraitementDate.UnixTimeStampToDateTime(tache.unixDebut);
            tache.Annee = Convert.ToString(DateDebut.Year);
            tache.Mois = Convert.ToString(DateDebut.Month - 1);
            tache.Jour = Convert.ToString(DateDebut.Day);

            tache.HRappel = (String.IsNullOrEmpty(tache.HRappel)) ? "00" : tache.HRappel;
            tache.mRappel = (String.IsNullOrEmpty(tache.mRappel)) ? "00" : tache.mRappel;
            TimeSpan tsRappel = new TimeSpan(
                Convert.ToInt32(tache.HRappel), Convert.ToInt32(tache.mRappel), 0
                );
            DateTime DateRappel = DateDebut.Subtract(tsRappel);

            if (DateRappel == DateDebut)
            {
                tache.DateRappelCalendrier = "Aucun";
            }
            else
            {
                tache.DateRappelCalendrier = TempsRappel(DateRappel);
            }

            tache.RecurrenceAffichage = Tache.Nomrecurrence(tache.Recurrence);
        }
    }
}
