﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InTime.Models;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace InTime.Controllers
{
    public class AjouterTacheController : Controller
    {
        int StrToInt(string nombre)
        {
            return Convert.ToInt32(nombre);
        }

        public List<SelectListItem> Les_Mois()
        {
            List<SelectListItem> mois = new List<SelectListItem>();
            mois.Add(new SelectListItem { Text = "Janvier", Value = "1" });
            mois.Add(new SelectListItem { Text = "Février", Value = "2" });
            mois.Add(new SelectListItem { Text = "Mars", Value = "3" });
            mois.Add(new SelectListItem { Text = "Avril", Value = "4" });
            mois.Add(new SelectListItem { Text = "Mai", Value = "5" });
            mois.Add(new SelectListItem { Text = "Juin", Value = "6" });
            mois.Add(new SelectListItem { Text = "Juillet", Value = "7" });
            mois.Add(new SelectListItem { Text = "Aout", Value = "8" });
            mois.Add(new SelectListItem { Text = "Septembre", Value = "9" });
            mois.Add(new SelectListItem { Text = "Octobre", Value = "10" });
            mois.Add(new SelectListItem { Text = "Novembre", Value = "11" });
            mois.Add(new SelectListItem { Text = "Décembre", Value = "12" });

            return mois;
        }


        public ActionResult Index()
        {

            if (User.Identity.IsAuthenticated)
            {
                InitialiseViewBags();

                return View();
            }
            else
            {
                return View(UrlErreur.Authentification);
            }
        }

        public JsonResult JourDuMois(int Year, string Month)
        {
            if (!String.IsNullOrEmpty(Month))
            {
                List<SelectListItem> jours = new List<SelectListItem>();
                if (!String.IsNullOrEmpty(Month))
                {
                    int nDays = DateTime.DaysInMonth(Year, StrToInt(Month));
                    for (int i = 1; i <= nDays; ++i)
                    {
                        jours.Add(new SelectListItem { Text = Convert.ToString(i), Value = Convert.ToString(i) });
                    }
                }

                return Json(new SelectList(jours.ToArray(), "Text", "Value"), JsonRequestBehavior.AllowGet);
            }
            else
            {

                return Json(null, JsonRequestBehavior.DenyGet);
            }
        }

        [HttpPost]
        public ActionResult Index(Tache model)
        {
            if (User.Identity.IsAuthenticated)
            {
                Validations(model);

                if (!ModelState.IsValid)
                {
                    InitialiseViewBags();

                    return View("Index");
                }
                else
                {
                    var message = "";
                    if (InsertionTache(model))
                    {
                        message = "Reussi";
                    }
                    else
                    {
                        message = "Erreur";
                    }
                    TempData["Message"] = message;

                    return RedirectToAction("Index", "AjouterTache");
                }
            }
            else
            {
                return View(UrlErreur.Authentification);
            }
        }

        private void Validations(Tache model)
        {
            const string strValidationMotContain = "Choisir";

            if ((model.Mois == null || model.Mois.Contains(strValidationMotContain)) ||
                (model.Annee == null) ||
                (model.Jour == null || model.Jour.Contains(strValidationMotContain)))
            {
                ModelState.AddModelError("Mois", "Veuillez compléter la date correctement.");
                ModelState.AddModelError("Annee", "");
                ModelState.AddModelError("Jour", "");
            }

            if (model.HDebut == null || model.mDebut == null)
            {
                ModelState.AddModelError("dbTacheHeure", "Veuillez compléter l'heure de début correctement.");
                ModelState.AddModelError("dbTacheMinute", "");
            }

            if (model.HFin == null || model.mFin == null)
            {
                ModelState.AddModelError("finTacheHeure", "Veuillez compléter l'heure de fin correctement.");
                ModelState.AddModelError("finTacheMinute", "");
            }
            else
            {
                ValHeureFinDebut(ref model);
            }

            if (model.HRappel != null && model.mRappel == null)
            {
                ModelState.AddModelError("rapTacheHeure", "Veuillez compléter l'heure de rappel correctement.");
                ModelState.AddModelError("rapTacheMinute", "");
            }
        }

        private void ValHeureFinDebut(ref Tache model)
        {
            const string strMessageErreur = "Vos heures ne sont pas valide";

            if (model.HDebut != null && model.mDebut != null)
            {
                if (StrToInt(model.HDebut) > StrToInt(model.HFin))
                {
                    ModelState.AddModelError("", strMessageErreur);
                }
                else
                {
                    if (StrToInt(model.HDebut) == StrToInt(model.HFin) &&
                        StrToInt(model.mDebut) >= StrToInt(model.mFin))
                    {
                        ModelState.AddModelError("", strMessageErreur);
                    }
                }
            }
        }

        private bool InsertionTache(Tache Model)
        {
            try
            {
                int UserId = Int32.Parse(InTime.Models.Cookie.ObtenirCookie(User.Identity.Name));
                string SqlInsert = "INSERT INTO Taches (UserId,NomTache,Lieu,Description,Mois,Jour,HDebut,HFin,mDebut,mFin,HRappel,mRappel,Annee,Reccurence)"
                    +" VALUES (@UserId,@NomTache,@Lieu,@Description,@Mois,@Jour,@HDebut,@HFin,@mDebut,@mFin,@HRappel,@mRappel,@Annee,@Reccurence);";

                List<SqlParameter> listParametre = new List<SqlParameter>();
                listParametre.Add(new SqlParameter("@UserId", UserId));
                listParametre.Add(new SqlParameter("@NomTache", Model.NomTache));
                listParametre.Add(new SqlParameter("@Lieu", Model.Lieu));
                listParametre.Add(new SqlParameter("@Description", Model.Description));
                listParametre.Add(new SqlParameter("@Mois", Model.Mois));
                listParametre.Add(new SqlParameter("@Jour", Model.Jour));
                listParametre.Add(new SqlParameter("@HDebut", Model.HDebut));
                listParametre.Add(new SqlParameter("@HFin", Model.HFin));
                listParametre.Add(new SqlParameter("@mDebut", Model.mDebut));
                listParametre.Add(new SqlParameter("@mFin", Model.mFin));
                listParametre.Add(new SqlParameter("@HRappel", Model.HRappel));
                listParametre.Add(new SqlParameter("@mRappel", Model.mRappel));
                listParametre.Add(new SqlParameter("@Annee", Model.Annee));
                listParametre.Add(new SqlParameter("@Reccurence", Model.Reccurence));

                //string SqlInsert = string.Format(@"INSERT INTO Taches (UserId,NomTache,Lieu,Description,Mois,Jour,HDebut,HFin,mDebut,mFin,HRappel,mRappel,Annee,Reccurence)"
                //    +" VALUES ({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}')",
                //   UserId, RequeteSql.EnleverApostrophe(Model.NomTache), RequeteSql.EnleverApostrophe(Model.Lieu), RequeteSql.EnleverApostrophe(Model.Description),
                //    Model.Mois, Model.Jour, Model.HDebut, Model.HFin, Model.mDebut, Model.mFin, Model.HRappel, Model.mRappel, Model.Annee,Model.Reccurence);

                return RequeteSql.ExecuteQuery(SqlInsert,listParametre);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void InitialiseViewBags()
        {
            ViewBag.trancheMin = new SelectList(Tache.tempsMinutes);

            ViewBag.trancheHeure = new SelectList(Tache.tempsHeure);

            ViewBag.MoisAnnee = new SelectList(Les_Mois(), "Value", "Text");

            ViewBag.Reccurence = new SelectList(Tache.options);
        }
    }
}
