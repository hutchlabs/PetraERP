﻿using PetraERP.Shared.Datasources;
using PetraERP.Shared.UI;
using System;
using System.Collections.Generic;
using System.Data.Linq.SqlClient;
using System.Linq;

namespace PetraERP.Shared.Models
{
    #region CRM DB

    public class CrmData
    {
        #region CRM DB Methods

        public static sla_timer GetSLA(int id)
        {
            return (from cat in Database.CRM.sla_timers where cat.ID == id select cat).Single();
        }

        public static void AddSLA(crmSLAView SelectedSLA)
        {
            sla_timer newCat = new sla_timer();
            newCat.sla_name = SelectedSLA.Name;
            newCat.code = SelectedSLA.code;
            newCat.pre_escalate = SelectedSLA.Pre_escalate;
            newCat.escalate = SelectedSLA.Escalated;
            newCat.Status = SelectedSLA.Active;
            newCat.owner = AppData.CurrentUser.id;
            newCat.created_at = DateTime.Now;
            Database.CRM.sla_timers.InsertOnSubmit(newCat);
            Database.CRM.SubmitChanges();
        }

        public static void SaveSLA(crmSLAView SelectedSLA)
        {
            sla_timer cat = CrmData.GetSLA(SelectedSLA.Id);
            cat.sla_name = SelectedSLA.Name;
            cat.code = SelectedSLA.code;
            cat.pre_escalate = SelectedSLA.Pre_escalate;
            cat.escalate = SelectedSLA.Escalated;
            cat.Status = SelectedSLA.Active;
            cat.updated_at = DateTime.Now;
            Database.CRM.SubmitChanges();
        }

        public static IEnumerable<crmSLAView> get_SLAs_View()
        {
            return (from sla in Database.CRM.sla_timers                   
                    select new crmSLAView() {code = sla.code, Id = sla.ID, Name = sla.sla_name, Pre_escalate = sla.pre_escalate, Escalated = sla.escalate, Active = (bool)sla.Status});
        }

        public static crmSLAView get_SLAs_View(int id)
        {
            return (from sla in Database.CRM.sla_timers
                    where sla.ID == id
                    select new crmSLAView() { code = sla.code, Id = sla.ID, Name = sla.sla_name, Pre_escalate = sla.pre_escalate, Escalated = sla.escalate, Active = (bool)sla.Status }).Single<crmSLAView>(); ;
        }

        public static crmSLAView get_SLAs_By_Name_View(string slaName)
        {
            return (from sla in Database.CRM.sla_timers
                    where sla.sla_name == slaName
                    select new crmSLAView() { code = sla.code, Id = sla.ID, Name = sla.sla_name, Pre_escalate = sla.pre_escalate, Escalated = sla.escalate, Active = (bool)sla.Status }).Single<crmSLAView>(); ;
        }


        public static correspondence GetCorrespondence(int id)
        {
            return (from cat in Database.CRM.correspondences where cat.id == id select cat).Single();
        }

        public static void AddCorrespondence(crmCorrespondenceView SelectedCorrespondence)
        {
            correspondence newCat = new correspondence();
            newCat.correspondence_name = SelectedCorrespondence.Name;
            newCat.code = SelectedCorrespondence.code;
            newCat.description = SelectedCorrespondence.description;
            newCat.category_id = SelectedCorrespondence.category_id;
            newCat.status = SelectedCorrespondence.active;
            newCat.owner = AppData.CurrentUser.id;
            newCat.created_at = DateTime.Now;
            Database.CRM.correspondences.InsertOnSubmit(newCat);
            Database.CRM.SubmitChanges();
        }

        public static void SaveCorrespondence(crmCorrespondenceView SelectedCorrespondence)
        {
            correspondence cat = CrmData.GetCorrespondence(SelectedCorrespondence.Id);
            cat.correspondence_name = SelectedCorrespondence.Name;
            cat.code = SelectedCorrespondence.code;
            cat.description = SelectedCorrespondence.description;
            cat.category_id = SelectedCorrespondence.category_id;
            cat.status = SelectedCorrespondence.active;
            cat.modified_by = AppData.CurrentUser.id;
            cat.updated_at = DateTime.Now;
            Database.CRM.SubmitChanges();
        }

        public static IEnumerable<crmCorrespondenceView> get_Correspondence()
        {
            return (from corres in Database.CRM.correspondences
                    join cats in Database.CRM.categories on corres.category_id equals cats.id
                    select new crmCorrespondenceView() { code = corres.code, Id = corres.id, Name = corres.correspondence_name, description = corres.description, category = cats.category_name, category_id = cats.id });

        }

        public static IEnumerable<crmCorrespondenceView> get_Correspondence_Filter_By_Category(int cat_id)
        {
            return (from corres in Database.CRM.correspondences
                    join cats in Database.CRM.categories on corres.category_id equals cats.id
                    where corres.category_id == cat_id
                    select new crmCorrespondenceView() { code = corres.code, Id = corres.id, Name = corres.correspondence_name, description = corres.description, category = cats.category_name, category_id = cats.id });
        }

        public static IEnumerable<crmCorrespondenceView> get_Active_Correspondence_Filter_By_Category(int cat_id)
        {
            return (from corres in Database.CRM.correspondences
                    join cats in Database.CRM.categories on corres.category_id equals cats.id
                    where corres.category_id == cat_id && corres.status == true
                    select new crmCorrespondenceView() { code = corres.code, Id = corres.id, Name = corres.correspondence_name, description = corres.description, category = cats.category_name, category_id = cats.id });
        }

        public static crmCorrespondenceView get_Correspondence(int id)
        {
            return (from corres in Database.CRM.correspondences
                    join cats in Database.CRM.categories on corres.category_id equals cats.id
                    where corres.id == id
                    select new crmCorrespondenceView() { code = corres.code, Id = corres.id, Name = corres.correspondence_name, description = corres.description, category = cats.category_name, category_id = cats.id }).Single<crmCorrespondenceView>();
        }


        public static sub_correspondence GetSubCorrespondence(int id)
        {
            return (from cat in Database.CRM.sub_correspondences where cat.id == id select cat).Single();
        }

        public static void AddSubCorrespondence(crmSubCorrespondenceView SelectedSubCorrespondence)
        {
            sub_correspondence newCat = new sub_correspondence();
            newCat.sub_correspondence_name = SelectedSubCorrespondence.Name;
            newCat.code = SelectedSubCorrespondence.code;
            newCat.description = SelectedSubCorrespondence.description;
            newCat.correspondence_id = SelectedSubCorrespondence.correspondence_id;
            newCat.sla_id = SelectedSubCorrespondence.sla_id;
            newCat.status = (bool)SelectedSubCorrespondence.active;
            newCat.owner = AppData.CurrentUser.id;
            newCat.created_at = DateTime.Now;
            Database.CRM.sub_correspondences.InsertOnSubmit(newCat);
            Database.CRM.SubmitChanges();
        }

        public static void SaveSubCorrespondence(crmSubCorrespondenceView SelectedSubCorrespondence)
        {
            sub_correspondence cat = CrmData.GetSubCorrespondence(SelectedSubCorrespondence.Id);
            cat.sub_correspondence_name = SelectedSubCorrespondence.Name;
            cat.code = SelectedSubCorrespondence.code;
            cat.description = SelectedSubCorrespondence.description;
            cat.correspondence_id = SelectedSubCorrespondence.correspondence_id;
            cat.sla_id = SelectedSubCorrespondence.sla_id;
            cat.status = SelectedSubCorrespondence.active;
            cat.modified_by = AppData.CurrentUser.id;
            cat.updated_at = DateTime.Now;
            Database.CRM.SubmitChanges();
        }

        public static IEnumerable<crmSubCorrespondenceView> get_Sub_Correspondence()
        {
            return (from subCorres in Database.CRM.sub_correspondences
                    join corres in Database.CRM.correspondences on subCorres.correspondence_id equals corres.id
                    from sla in Database.CRM.sla_timers
                    where subCorres.sla_id == sla.ID
                    select new crmSubCorrespondenceView() {code = subCorres.code, Id = subCorres.id, Name = subCorres.sub_correspondence_name, description = subCorres.description, correspondence = corres.correspondence_name, SLA = sla.sla_name, correspondence_id=corres.id, sla_id=sla.ID, active = (bool)subCorres.status });

        }

        public static crmSubCorrespondenceView get_Sub_Correspondence(int id)
        {
            return (from subCorres in Database.CRM.sub_correspondences
                    join corres in Database.CRM.correspondences on subCorres.correspondence_id equals corres.id
                    from sla in Database.CRM.sla_timers
                    where subCorres.sla_id == sla.ID && subCorres.id == id
                    select new crmSubCorrespondenceView() { code = subCorres.code, Id = subCorres.id, Name = subCorres.sub_correspondence_name, description = subCorres.description, correspondence = corres.correspondence_name, SLA = sla.sla_name, correspondence_id = corres.id, sla_id = sla.ID, active = (bool)subCorres.status }).Single<crmSubCorrespondenceView>();
        }

        public static IEnumerable<crmSubCorrespondenceView> get_Sub_Correspondence_Filter_By_Correspondence(int corres_id)
        {
            return (from subCorres in Database.CRM.sub_correspondences
                    join corres in Database.CRM.correspondences on subCorres.correspondence_id equals corres.id
                    where  subCorres.correspondence_id == corres_id
                    select new crmSubCorrespondenceView() { code = subCorres.code, Id = subCorres.id, Name = subCorres.sub_correspondence_name, description = subCorres.description, correspondence = corres.correspondence_name, SLA = "", correspondence_id = corres.id, sla_id = 0, active = (bool)subCorres.status });

        }

        public static IEnumerable<crmSubCorrespondenceView> get_Active_Sub_Correspondence_Filter_By_Correspondence(int corres_id)
        {
            return (from subCorres in Database.CRM.sub_correspondences
                    join corres in Database.CRM.correspondences on subCorres.correspondence_id equals corres.id
                    where subCorres.correspondence_id == corres_id && subCorres.status == true
                    select new crmSubCorrespondenceView() { code = subCorres.code, Id = subCorres.id, Name = subCorres.sub_correspondence_name, description = subCorres.description, correspondence = corres.correspondence_name, SLA = "", correspondence_id = corres.id, sla_id = 0, active = (bool)subCorres.status });

        }

       
        public static IEnumerable<crmCategoryView> get_Categories()
        {
            return (from cat in Database.CRM.categories                
                    select new crmCategoryView() { code = cat.code,  Id = cat.id, Name = cat.category_name, description = cat.description, active = (bool)cat.status });

        }

        public static IEnumerable<crmCategoryView> get_Active_Categories()
        {
            return (from cat in Database.CRM.categories
                    where cat.status == true
                    select new crmCategoryView() { code = cat.code, Id = cat.id, Name = cat.category_name, description = cat.description, active = (bool)cat.status });

        }

        public static crmCategoryView get_Categories(int id)
        {
            return (from cat in Database.CRM.categories                  
                    where cat.id == id
                    select new crmCategoryView(){ code = cat.code, Id = cat.id, Name = cat.category_name, description = cat.description, active = (bool)cat.status }).Single<crmCategoryView>();
        }

        public static category GetCategory(int id)
        {
            return (from cat in Database.CRM.categories where cat.id == id select cat).Single();
        }

        public static void AddCategory(crmCategoryView SelectedCategory)
        {
            category newCat = new category(); 
            newCat.category_name = SelectedCategory.Name;
            newCat.code = SelectedCategory.code;
            newCat.description = SelectedCategory.description;
            newCat.status = (bool)SelectedCategory.active;
            newCat.owner = AppData.CurrentUser.id;
            newCat.created_at = DateTime.Now;
            Database.CRM.categories.InsertOnSubmit(newCat);
            Database.CRM.SubmitChanges();
        }

        public static void SaveCategory(crmCategoryView SelectedCategory)
        {
            category cat = CrmData.GetCategory(SelectedCategory.Id);
            cat.category_name = SelectedCategory.Name;
            cat.code = SelectedCategory.code;
            cat.description = SelectedCategory.description;
            cat.status = (bool)SelectedCategory.active;
            cat.modified_by = AppData.CurrentUser.id;
            cat.updated_at = DateTime.Now;
            Database.CRM.SubmitChanges();
        }
       
        
        public static string get_ticket_seqence_no(int cat_id, int corres_id, int sub_corres_id, int month, int year)
        {
            return (from ini in Database.CRM.tickets
                    where ini.ticket_month == month &&
                          ini.ticket_year == year
                          select ini).Count().ToString("00000");
       
        }


        private static string[] do_entity_search(string term)
        {
            List<string> codes = new List<string>();
            //var people = search_customer_by_name(term);
            //var companies = search_companies_by_name(term);
            // foreach (var p in people) { codes.Add(p.Petra_ID); }
            //foreach (var c in companies) { codes.Add(c.petra_id); }
            return codes.ToArray();
        }

        private static int[] do_username_search(string term)
        {
            List<int> ids = new List<int>();
            var people = Users.SearchUserByUsername(term);
            foreach (var p in people) { ids.Add(p.id); }
            return ids.ToArray();
        }

        public static IEnumerable<crmTicketsView> search_tickets(string term, int filter=-1)
        {
            if (term.Equals(""))
            {
                return null;
            }
            else
            {
                string[] hicodes = do_entity_search(term);
                int[] ids = do_username_search(term);

                term = "%" + term + "%";

                if (Users.IsCurrentUserCRMAdmin())
                {
                    #region crm admin search
                    
                    if (filter == 0)
                    {
                        return (from tic in Database.CRM.tickets
                                join cat in Database.CRM.categories on tic.category_id equals cat.id
                                join corress in Database.CRM.correspondences on tic.correspondence_id equals corress.id
                                join sub_corress in Database.CRM.sub_correspondences on corress.id equals sub_corress.correspondence_id
                                join tic_status in Database.CRM.ticket_statuses on tic.status equals tic_status.id                               
                                from sla in Database.CRM.sla_timers
                                where (sla.ID == sub_corress.sla_id && corress.id == tic.correspondence_id && sub_corress.id == tic.sub_correspondence_id && cat.id == tic.category_id) &&
                                      (
                                         SqlMethods.Like(tic.ticket_id, term) || SqlMethods.Like(tic.customer_id, term) ||
                                         SqlMethods.Like(tic.subject, term) || SqlMethods.Like(corress.correspondence_name, term) ||
                                         SqlMethods.Like(sub_corress.sub_correspondence_name, term) || SqlMethods.Like(cat.category_name, term) ||
                                         ids.Contains((tic.assigned_to ?? 0)) || SqlMethods.Like(cat.category_name, term) ||
                                         hicodes.Contains(tic.customer_id)
                                      )
                                orderby tic.created_at.AddMinutes(sla.escalate) descending
                                select new crmTicketsView() { ticket_id = tic.ticket_id, category = cat.category_name, correspondence = corress.correspondence_name, subject = tic.subject, status = tic_status.status_desc, subcorrespondence = sub_corress.sub_correspondence_name, created_at = GetFormattedDate(tic.created_at), Assigned_To = GetUserName(tic.assigned_to ?? 0), escalation_due = GetDueDate(tic.created_at, sla.escalate) }).Distinct();
                    }
                    else if (filter == -1)
                    {
                        var loadDef = (from s in Database.CRM.ticket_statuses where s.is_default == true select s.id).ToArray();
                        return (from tic in Database.CRM.tickets
                                join cat in Database.CRM.categories on tic.category_id equals cat.id
                                join corress in Database.CRM.correspondences on tic.correspondence_id equals corress.id
                                join sub_corress in Database.CRM.sub_correspondences on corress.id equals sub_corress.correspondence_id
                                join tic_status in Database.CRM.ticket_statuses on tic.status equals tic_status.id
                                from sla in Database.CRM.sla_timers
                                where loadDef.Contains(tic.status)
                                where (sla.ID == sub_corress.sla_id && corress.id == tic.correspondence_id && sub_corress.id == tic.sub_correspondence_id && cat.id == tic.category_id) &&
                                      (
                                         SqlMethods.Like(tic.ticket_id, term) || SqlMethods.Like(tic.customer_id, term) ||
                                         SqlMethods.Like(tic.subject, term) || SqlMethods.Like(corress.correspondence_name, term) ||
                                         SqlMethods.Like(sub_corress.sub_correspondence_name, term) || SqlMethods.Like(cat.category_name, term) ||
                                         ids.Contains((tic.assigned_to ?? 0)) || SqlMethods.Like(cat.category_name, term) ||
                                         hicodes.Contains(tic.customer_id)
                                      )
                                orderby tic.created_at.AddMinutes(sla.escalate) descending
                                select new crmTicketsView() { ticket_id = tic.ticket_id, category = cat.category_name, correspondence = corress.correspondence_name, subject = tic.subject, status = tic_status.status_desc, subcorrespondence = sub_corress.sub_correspondence_name, created_at = GetFormattedDate(tic.created_at), Assigned_To = GetUserName(tic.assigned_to ?? 0), escalation_due = GetDueDate(tic.created_at, sla.escalate) }).Distinct();
                    }
                    else
                    {
                        var loadDef = (from s in Database.CRM.ticket_statuses where s.is_default == true select s.id).ToArray();
                        return (from tic in Database.CRM.tickets
                                join cat in Database.CRM.categories on tic.category_id equals cat.id
                                join corress in Database.CRM.correspondences on tic.correspondence_id equals corress.id
                                join sub_corress in Database.CRM.sub_correspondences on corress.id equals sub_corress.correspondence_id
                                join tic_status in Database.CRM.ticket_statuses on tic.status equals tic_status.id
                                from sla in Database.CRM.sla_timers
                                where (tic.status == filter && sla.ID == sub_corress.sla_id && corress.id == tic.correspondence_id && sub_corress.id == tic.sub_correspondence_id && cat.id == tic.category_id) &&
                                      (
                                         SqlMethods.Like(tic.ticket_id, term) || SqlMethods.Like(tic.customer_id, term) ||
                                         SqlMethods.Like(tic.subject, term) || SqlMethods.Like(corress.correspondence_name, term) ||
                                         SqlMethods.Like(sub_corress.sub_correspondence_name, term) || SqlMethods.Like(cat.category_name, term) ||
                                         ids.Contains((tic.assigned_to ?? 0)) || SqlMethods.Like(cat.category_name, term) ||
                                         hicodes.Contains(tic.customer_id)
                                      )
                                orderby tic.created_at.AddMinutes(sla.escalate) descending
                                select new crmTicketsView() { ticket_id = tic.ticket_id, category = cat.category_name, correspondence = corress.correspondence_name, subject = tic.subject, status = tic_status.status_desc, subcorrespondence = sub_corress.sub_correspondence_name, created_at = GetFormattedDate(tic.created_at), Assigned_To = GetUserName(tic.assigned_to ?? 0), escalation_due = GetDueDate(tic.created_at, sla.escalate) }).Distinct();
                   
                    }
                    #endregion 
                }
                else
                {
                    #region normal user search

                    int uid = AppData.CurrentUser.id;

                    if (filter == 0)
                    {
                        return (from tic in Database.CRM.tickets
                                join cat in Database.CRM.categories on tic.category_id equals cat.id
                                join corress in Database.CRM.correspondences on tic.correspondence_id equals corress.id
                                join sub_corress in Database.CRM.sub_correspondences on corress.id equals sub_corress.correspondence_id
                                join tic_status in Database.CRM.ticket_statuses on tic.status equals tic_status.id 
                                from sla in Database.CRM.sla_timers
                                where ((tic.assigned_to == uid || tic.assigned_to == null) && sla.ID == sub_corress.sla_id && corress.id == tic.correspondence_id && sub_corress.id == tic.sub_correspondence_id && cat.id == tic.category_id) &&
                                      (
                                         SqlMethods.Like(tic.ticket_id, term) || SqlMethods.Like(tic.customer_id, term) ||
                                         SqlMethods.Like(tic.subject, term) || SqlMethods.Like(corress.correspondence_name, term) ||
                                         SqlMethods.Like(sub_corress.sub_correspondence_name, term) || SqlMethods.Like(cat.category_name, term) ||
                                         ids.Contains((tic.assigned_to ?? 0)) || SqlMethods.Like(cat.category_name, term) ||
                                         hicodes.Contains(tic.customer_id)
                                      )
                                orderby tic.created_at.AddMinutes(sla.escalate) descending
                                select new crmTicketsView() { ticket_id = tic.ticket_id, category = cat.category_name, correspondence = corress.correspondence_name, subject = tic.subject, status = tic_status.status_desc, subcorrespondence = sub_corress.sub_correspondence_name, created_at = GetFormattedDate(tic.created_at), Assigned_To = GetUserName(tic.assigned_to ?? 0), escalation_due = GetDueDate(tic.created_at, sla.escalate) }).Distinct();
                    }
                    else if (filter == -1)
                    {
                        var loadDef = (from s in Database.CRM.ticket_statuses where s.is_default == true select s.id).ToArray();
                        return (from tic in Database.CRM.tickets
                                join cat in Database.CRM.categories on tic.category_id equals cat.id
                                join corress in Database.CRM.correspondences on tic.correspondence_id equals corress.id
                                join sub_corress in Database.CRM.sub_correspondences on corress.id equals sub_corress.correspondence_id
                                join tic_status in Database.CRM.ticket_statuses on tic.status equals tic_status.id
                                from sla in Database.CRM.sla_timers
                                where loadDef.Contains(tic.status)
                                where ((tic.assigned_to == uid || tic.assigned_to == null) && sla.ID == sub_corress.sla_id && corress.id == tic.correspondence_id && sub_corress.id == tic.sub_correspondence_id && cat.id == tic.category_id) &&
                                      (
                                         SqlMethods.Like(tic.ticket_id, term) || SqlMethods.Like(tic.customer_id, term) ||
                                         SqlMethods.Like(tic.subject, term) || SqlMethods.Like(corress.correspondence_name, term) ||
                                         SqlMethods.Like(sub_corress.sub_correspondence_name, term) || SqlMethods.Like(cat.category_name, term) ||
                                         ids.Contains((tic.assigned_to ?? 0)) || SqlMethods.Like(cat.category_name, term) ||
                                         hicodes.Contains(tic.customer_id)
                                      )
                                orderby tic.created_at.AddMinutes(sla.escalate) descending
                                select new crmTicketsView() { ticket_id = tic.ticket_id, category = cat.category_name, correspondence = corress.correspondence_name, subject = tic.subject, status = tic_status.status_desc, subcorrespondence = sub_corress.sub_correspondence_name, created_at = GetFormattedDate(tic.created_at), Assigned_To = GetUserName(tic.assigned_to ?? 0), escalation_due = GetDueDate(tic.created_at, sla.escalate) }).Distinct();
                    }
                    else
                    {
                        var loadDef = (from s in Database.CRM.ticket_statuses where s.is_default == true select s.id).ToArray();
                        return (from tic in Database.CRM.tickets
                                join cat in Database.CRM.categories on tic.category_id equals cat.id
                                join corress in Database.CRM.correspondences on tic.correspondence_id equals corress.id
                                join sub_corress in Database.CRM.sub_correspondences on corress.id equals sub_corress.correspondence_id
                                join tic_status in Database.CRM.ticket_statuses on tic.status equals tic_status.id
                                from sla in Database.CRM.sla_timers
                                where ((tic.assigned_to == uid || tic.assigned_to == null) && tic.status == filter && sla.ID == sub_corress.sla_id && corress.id == tic.correspondence_id && sub_corress.id == tic.sub_correspondence_id && cat.id == tic.category_id) &&
                                      (
                                         SqlMethods.Like(tic.ticket_id, term) || SqlMethods.Like(tic.customer_id, term) ||
                                         SqlMethods.Like(tic.subject, term) || SqlMethods.Like(corress.correspondence_name, term) ||
                                         SqlMethods.Like(sub_corress.sub_correspondence_name, term) || SqlMethods.Like(cat.category_name, term) ||
                                         ids.Contains((tic.assigned_to ?? 0)) || SqlMethods.Like(cat.category_name, term) ||
                                         hicodes.Contains(tic.customer_id)
                                      )
                                orderby tic.created_at.AddMinutes(sla.escalate) descending
                                select new crmTicketsView() { ticket_id = tic.ticket_id, category = cat.category_name, correspondence = corress.correspondence_name, subject = tic.subject, status = tic_status.status_desc, subcorrespondence = sub_corress.sub_correspondence_name, created_at = GetFormattedDate(tic.created_at), Assigned_To = GetUserName(tic.assigned_to ?? 0), escalation_due = GetDueDate(tic.created_at, sla.escalate) }).Distinct();
                   
                    }

                    #endregion 
                }
            }
        }

        public static IEnumerable<crmTicketsView> get_active_tickets(int status_id=-1)
        {
            //Default statuses to load
            var loadDef = (from s in Database.CRM.ticket_statuses where s.is_default == true select s.id).ToArray();

            if (Users.IsCurrentUserCRMAdmin())
            {
                if (status_id == 0)
                {
                    return (from tic in Database.CRM.tickets
                            join cat in Database.CRM.categories on tic.category_id equals cat.id
                            join corress in Database.CRM.correspondences on tic.correspondence_id equals corress.id
                            join sub_corress in Database.CRM.sub_correspondences on corress.id equals sub_corress.correspondence_id 
                            join tic_status in Database.CRM.ticket_statuses on tic.status equals tic_status.id
                            from sla in Database.CRM.sla_timers                   
                            where sla.ID == sub_corress.sla_id && corress.id == tic.correspondence_id && sub_corress.id == tic.sub_correspondence_id && cat.id == tic.category_id
                            orderby tic.created_at.AddMinutes(sla.escalate) descending
                            select new crmTicketsView() { ticket_id = tic.ticket_id, category = cat.category_name, correspondence = corress.correspondence_name, subject = tic.subject, status = tic_status.status_desc, subcorrespondence = sub_corress.sub_correspondence_name, created_at = GetFormattedDate(tic.created_at), Assigned_To = GetUserName(tic.assigned_to ?? 0), escalation_due = GetDueDate(tic.created_at, sla.escalate) }).Distinct();
                }
                else if(status_id == -1)
                {
                     return (from tic in Database.CRM.tickets
                            join cat in Database.CRM.categories on tic.category_id equals cat.id
                            join corress in Database.CRM.correspondences on tic.correspondence_id equals corress.id
                            join sub_corress in Database.CRM.sub_correspondences on corress.id equals sub_corress.correspondence_id 
                            join tic_status in Database.CRM.ticket_statuses on tic.status equals tic_status.id
                            from sla in Database.CRM.sla_timers
                            where loadDef.Contains(tic.status)
                            where sla.ID == sub_corress.sla_id && corress.id == tic.correspondence_id && sub_corress.id == tic.sub_correspondence_id && cat.id == tic.category_id
                            orderby tic.created_at.AddMinutes(sla.escalate) descending
                            select new crmTicketsView() { ticket_id = tic.ticket_id, category = cat.category_name, correspondence = corress.correspondence_name, subject = tic.subject, status = tic_status.status_desc, subcorrespondence = sub_corress.sub_correspondence_name, created_at = GetFormattedDate(tic.created_at), Assigned_To = GetUserName(tic.assigned_to ?? 0), escalation_due = GetDueDate(tic.created_at, sla.escalate) }).Distinct();
        
                }
                else
                {
                    return (from tic in Database.CRM.tickets
                            join cat in Database.CRM.categories on tic.category_id equals cat.id
                            join corress in Database.CRM.correspondences on tic.correspondence_id equals corress.id
                            join sub_corress in Database.CRM.sub_correspondences on corress.id equals sub_corress.correspondence_id
                            join tic_status in Database.CRM.ticket_statuses on tic.status equals tic_status.id
                            from sla in Database.CRM.sla_timers
                            where tic.status == status_id && sla.ID == sub_corress.sla_id && corress.id == tic.correspondence_id && sub_corress.id == tic.sub_correspondence_id && cat.id == tic.category_id
                            orderby tic.created_at.AddMinutes(sla.escalate) descending
                            select new crmTicketsView() { ticket_id = tic.ticket_id, category = cat.category_name, correspondence = corress.correspondence_name, subject = tic.subject, status = tic_status.status_desc, subcorrespondence = sub_corress.sub_correspondence_name, created_at = GetFormattedDate(tic.created_at), Assigned_To = GetUserName(tic.assigned_to ?? 0), escalation_due = GetDueDate(tic.created_at, sla.escalate) }).Distinct();
                }
            }
            else
            {
                int uid = AppData.CurrentUser.id;

                if (status_id == 0)
                {
                    return (from tic in Database.CRM.tickets
                            join cat in Database.CRM.categories on tic.category_id equals cat.id
                            join corress in Database.CRM.correspondences on tic.correspondence_id equals corress.id
                            join sub_corress in Database.CRM.sub_correspondences on corress.id equals sub_corress.correspondence_id
                            join tic_status in Database.CRM.ticket_statuses on tic.status equals tic_status.id
                            from sla in Database.CRM.sla_timers
                            where (tic.assigned_to == uid || tic.assigned_to == null) && sla.ID == sub_corress.sla_id && corress.id == tic.correspondence_id && sub_corress.id == tic.sub_correspondence_id && cat.id == tic.category_id
                            orderby tic.created_at.AddMinutes(sla.escalate) descending
                            select new crmTicketsView() { ticket_id = tic.ticket_id, category = cat.category_name, correspondence = corress.correspondence_name, subject = tic.subject, status = tic_status.status_desc, subcorrespondence = sub_corress.sub_correspondence_name, created_at = GetFormattedDate(tic.created_at), Assigned_To = GetUserName(tic.assigned_to ?? 0), escalation_due = GetDueDate(tic.created_at, sla.escalate) }).Distinct();
                }
                else if(status_id == -1)
                {
                     return (from tic in Database.CRM.tickets
                            join cat in Database.CRM.categories on tic.category_id equals cat.id
                            join corress in Database.CRM.correspondences on tic.correspondence_id equals corress.id
                            join sub_corress in Database.CRM.sub_correspondences on corress.id equals sub_corress.correspondence_id 
                            join tic_status in Database.CRM.ticket_statuses on tic.status equals tic_status.id
                            from sla in Database.CRM.sla_timers
                            where loadDef.Contains(tic.status)
                            where (tic.assigned_to == uid || tic.assigned_to == null) && sla.ID == sub_corress.sla_id && corress.id == tic.correspondence_id && sub_corress.id == tic.sub_correspondence_id && cat.id == tic.category_id
                            orderby tic.created_at.AddMinutes(sla.escalate) descending
                            select new crmTicketsView() { ticket_id = tic.ticket_id, category = cat.category_name, correspondence = corress.correspondence_name, subject = tic.subject, status = tic_status.status_desc, subcorrespondence = sub_corress.sub_correspondence_name, created_at = GetFormattedDate(tic.created_at), Assigned_To = GetUserName(tic.assigned_to ?? 0), escalation_due = GetDueDate(tic.created_at, sla.escalate) }).Distinct();
        
                }
                else
                {
                    return (from tic in Database.CRM.tickets
                            join cat in Database.CRM.categories on tic.category_id equals cat.id
                            join corress in Database.CRM.correspondences on tic.correspondence_id equals corress.id
                            join sub_corress in Database.CRM.sub_correspondences on corress.id equals sub_corress.correspondence_id
                            join tic_status in Database.CRM.ticket_statuses on tic.status equals tic_status.id
                            from sla in Database.CRM.sla_timers
                            where tic.status == status_id && (tic.assigned_to == uid || tic.assigned_to == null) && sla.ID == sub_corress.sla_id && corress.id == tic.correspondence_id && sub_corress.id == tic.sub_correspondence_id && cat.id == tic.category_id
                            orderby tic.created_at.AddMinutes(sla.escalate) descending
                            select new crmTicketsView() { ticket_id = tic.ticket_id, category = cat.category_name, correspondence = corress.correspondence_name, subject = tic.subject, status = tic_status.status_desc, subcorrespondence = sub_corress.sub_correspondence_name, created_at = GetFormattedDate(tic.created_at), Assigned_To = GetUserName(tic.assigned_to ?? 0), escalation_due = GetDueDate(tic.created_at, sla.escalate) }).Distinct();
                }
            }
        }

        public static crmTicketDetails get_ticket(int id)
        {
            return (from tic in Database.CRM.tickets
                    join cat in Database.CRM.categories on tic.category_id equals cat.id
                    join corress in Database.CRM.correspondences on tic.correspondence_id equals corress.id
                    join sub_corress in Database.CRM.sub_correspondences on tic.sub_correspondence_id equals sub_corress.id
                    join sla in Database.CRM.sla_timers on sub_corress.sla_id equals sla.ID
                    where tic.id == id
                    select new crmTicketDetails() { id = tic.id, owner = GetUserName(tic.owner), ownerid = tic.owner, category = cat.category_name, correspondence = corress.correspondence_name, notes = tic.notes, ticket_id = tic.ticket_id, petra_id = tic.customer_id, subcorrespondence = sub_corress.sub_correspondence_name, esacalation = sla.escalate, ticket_date = GetFormattedDate(tic.created_at), subject = tic.subject, customer_type = tic.customer_id_type, status_id = tic.status, contact_no = tic.contact_no, email = tic.email }
                  ).Single<crmTicketDetails>();
        }

        public static  crmTicketDetails get_ticket_details(string ticket_id)
        {
            return (from tic in Database.CRM.tickets
                    join cat in Database.CRM.categories on tic.category_id equals cat.id
                    join corress in Database.CRM.correspondences on tic.correspondence_id equals corress.id
                    join sub_corress in Database.CRM.sub_correspondences on tic.sub_correspondence_id equals sub_corress.id
                    join sla in Database.CRM.sla_timers on sub_corress.sla_id equals sla.ID
                    where tic.ticket_id == ticket_id
                    select new crmTicketDetails() { id = tic.id, assigned_to = tic.assigned_to ?? 0, owner = GetUserName(tic.owner), ownerid = tic.owner, category = cat.category_name, correspondence = corress.correspondence_name, notes = tic.notes, ticket_id = tic.ticket_id, petra_id = tic.customer_id, subcorrespondence = sub_corress.sub_correspondence_name, esacalation = sla.escalate, ticket_date = GetFormattedDate(tic.created_at), subject = tic.subject, customer_type = tic.customer_id_type, status_id = tic.status, contact_no = tic.contact_no, email = tic.email }
                   ).Single<crmTicketDetails>();
        }

        public static IEnumerable<crmTicketsView> get_customer_active_tickets(string petra_id)
        {
            try
            {
                return (from tic in Database.CRM.tickets
                        join cat in Database.CRM.categories on tic.category_id equals cat.id
                        join corress in Database.CRM.correspondences on tic.correspondence_id equals corress.id
                        join sub_corress in Database.CRM.sub_correspondences on corress.id equals sub_corress.correspondence_id
                        join tic_status in Database.CRM.ticket_statuses on tic.status equals tic_status.id
                        join sla in Database.CRM.sla_timers on sub_corress.sla_id equals sla.ID
                        where tic.customer_id == petra_id
                        orderby tic.created_at.AddMinutes(sla.escalate) descending
                        select new crmTicketsView() { ticket_id = tic.ticket_id, category = cat.category_name, correspondence = corress.correspondence_name, subject = tic.subject, status = tic_status.status_desc, subcorrespondence = sub_corress.sub_correspondence_name, created_at = GetFormattedDate(tic.created_at), Assigned_To = GetUserName(tic.assigned_to ?? 0), escalation_due = GetDueDate(tic.created_at, sla.escalate) }).Distinct();
            }
            catch (Exception e) { throw (e);  }
         }

        public static IEnumerable<crmTicketComments> get_ticket_comments(string ticket_id)
        {
            return (from tic in Database.CRM.tickets
                    join comm in Database.CRM.ticket_comments on tic.ticket_id equals comm.ticket_id
                   where comm.ticket_id == ticket_id
                   select new crmTicketComments() { comment_date = comm.comment_date.ToString(), comment = comm.comment, owner = GetUserName(comm.owner) });
        }

        public static IEnumerable<crmTicketStatus> get_ticket_status()
        {
            return (from tic_status in Database.CRM.ticket_statuses 
                    select new crmTicketStatus() { id = tic_status.id, status = tic_status.status_desc });
        }

        public static IEnumerable<crmTicketStatus> get_allowed_ticket_status()
        {
            IList<crmTicketStatus> c = new List<crmTicketStatus>(); 
            var ts = (from tic_status in Database.CRM.ticket_statuses
                                            where tic_status.can_set == true
                                            select tic_status);

            foreach(ticket_statuse t in ts)
            {
                if (t.status_desc=="ON HOLD")
                {
                    if (Users.IsCurrentUserCRMAdmin())
                    {
                       c.Add(new crmTicketStatus() { id = t.id, status = t.status_desc });
                    }
                } else {
                    c.Add(new crmTicketStatus() { id = t.id, status = t.status_desc });
                }
            }

            return c.AsEnumerable();
        }

        #endregion

        #region Microgen Control Functions

        public static IEnumerable<crmCustomerDetails> search_customer_by_petra_id(string p_id)
        {
            return (from ep in Database.Microgen.EntityPersons
                    join et in Database.Microgen.Entities on ep.EntityID equals et.EntityID
                    where et.EntityKey.Contains(p_id)
                    select new crmCustomerDetails() { Petra_ID = et.EntityKey, Customer_Name = (ep.FirstName + " " + ep.SecondNames + " " + ep.Surname), SSNIT_No = ep.NationalInsuranceNo });

        }

        public static crmCustomerDetails search_cust_by_petra_id(string p_id)
        {
            return (from ep in Database.Microgen.EntityPersons
                    join et in Database.Microgen.Entities on ep.EntityID equals et.EntityID
                    where et.EntityKey == p_id
                    select new crmCustomerDetails() { Petra_ID = et.EntityKey, Customer_Name = (ep.FirstName + " " + ep.SecondNames + " " + ep.Surname), SSNIT_No = ep.NationalInsuranceNo }).Single<crmCustomerDetails>();

        }

        public static IEnumerable<crmCustomerDetails> search_customer_by_name(string customer_name)
        {
            return (from er in Database.Microgen.EntityRoles
                    join ep in Database.Microgen.EntityPersons on er.EntityID equals ep.EntityID
                    join et in Database.Microgen.Entities on ep.EntityID equals et.EntityID
                    join emp in Database.Microgen.Associations on er.EntityID equals emp.TargetEntityID
                    join ec in Database.Microgen.EntityClients on emp.SourceEntityID equals ec.EntityID
                    where er.RoleTypeID == 3 && et.StatusID == 51004 && (new[] { "1001", "1004" }).Contains(emp.RoleTypeID.ToString())
                    && ((ep.FirstName ?? "") + " " + (ep.SecondNames + " " ?? "") + (ep.Surname ?? "")).Contains(customer_name) ||
                    ((ep.FirstName ?? "") + " " + (ep.Surname ?? "") + (" " + ep.SecondNames ?? "")).Contains(customer_name) ||
                    ((ep.Surname ?? "") + " " + (ep.SecondNames + " " ?? "") + (ep.FirstName ?? "")).Contains(customer_name) ||
                    ((ep.Surname ?? "") + " " + (ep.FirstName + " " ?? "") + (ep.SecondNames ?? "")).Contains(customer_name) ||
                    ((ep.SecondNames ?? "") + " " + (ep.FirstName + " " ?? "") + (ep.Surname ?? "")).Contains(customer_name) ||
                    ((ep.SecondNames ?? "") + " " + (ep.Surname + " " ?? "") + (ep.FirstName ?? "")).Contains(customer_name)
                    select new crmCustomerDetails() { Petra_ID = et.EntityKey, Customer_Name = ((ep.FirstName ?? "") + " " + (ep.SecondNames ?? "") + " " + (ep.Surname ?? "")), SSNIT_No = ep.NationalInsuranceNo, Employer = ec.FullName }).Distinct();

        }

        public static IEnumerable<crmCustomerDetails> search_customer_by_ssnit_no(string ssnit_no)
        {

            return (from ep in Database.Microgen.EntityPersons
                    join et in Database.Microgen.Entities on ep.EntityID equals et.EntityID
                    where ep.NationalInsuranceNo == ssnit_no
                    select new crmCustomerDetails() { Petra_ID = et.EntityKey, Customer_Name = ((ep.FirstName ?? "") + " " + (ep.SecondNames ?? "") + " " + (ep.Surname ?? "")), SSNIT_No = ep.NationalInsuranceNo });

        }

        public static IEnumerable<crmCustomerEmpoyerDetails> get_customer_employers(int entity_ID)
        {
            return (from emp in Database.Microgen.Associations
                    join ec in Database.Microgen.EntityClients on emp.SourceEntityID equals ec.EntityID
                    where (new[] { "1001", "1004" }).Contains(emp.RoleTypeID.ToString()) && emp.TargetEntityID == entity_ID
                    select new crmCustomerEmpoyerDetails() { Employer_Name = ec.FullName });
        }

        public static IEnumerable<crmCustomerContactNoDetails> get_customer_contact_nos(int entity_ID)
        {
            return (from ec in Database.Microgen.EntityContacts
                    where ec.EntityID == entity_ID
                    select new crmCustomerContactNoDetails() {  Contact_No = ec.TelephoneNo });
        }

        public static IEnumerable<crmCustomerEmailsDetails> get_customer_emails(int entity_ID)
        {
            return (from ec in Database.Microgen.EntityContacts
                    where ec.EntityID == entity_ID
                    select new crmCustomerEmailsDetails() { Email = ec.Email});
        }

        public static crmCustomerFullDetails get_customer_by_petra_id(string p_id)
        {
            return (from er in Database.Microgen.EntityRoles
                    join ep in Database.Microgen.EntityPersons on er.EntityID equals ep.EntityID
                    join et in Database.Microgen.Entities on ep.EntityID equals et.EntityID
                    join emp in Database.Microgen.Associations on er.EntityID equals emp.TargetEntityID
                    join ec in Database.Microgen.EntityClients on emp.SourceEntityID equals ec.EntityID
                    where er.RoleTypeID == 3 && et.StatusID == 51004 && (new[] { "1001", "1004" }).Contains(emp.RoleTypeID.ToString())
                    && et.EntityKey == p_id
                    select new crmCustomerFullDetails() { Entity_ID = ep.EntityID, Petra_ID = et.EntityKey, First_Name = ep.FirstName, Second_Name = ep.SecondNames, Last_Name = ep.Surname, SSNIT_No = ep.NationalInsuranceNo, Email = "econs.Email", Telephone = "econs.MobileNo", Employer = "Unknown" }).Take(1).Single<crmCustomerFullDetails>();

        }

        public static crmCustomerContactDetails get_customer_contact_by_id(int entity_id)
        {
            return (from ec in Database.Microgen.EntityContacts
                    where ec.EntityID == entity_id
                    select new crmCustomerContactDetails() { email = ec.Email, phone = ec.MobileNo }).Single<crmCustomerContactDetails>();

        }

       

        public static IEnumerable<crmCompanyList> search_companies_by_name(string company_name)
        {
            return (from ec in Database.Microgen.EntityClients
                    join et in Database.Microgen.Entities on ec.EntityID equals et.EntityID
                    join er in Database.Microgen.EntityRoles on et.EntityID equals er.EntityID
                    where et.StatusID == 51004 && et.EntityTypeID == 2 && er.RoleTypeID == 1001 && ec.FullName.Contains(company_name)
                    select new crmCompanyList() { petra_id = et.EntityKey, company_name = ec.FullName });

        }

        public static crmCompanyDetails get_company_by_petra_id(string petra_id)
        {
            return (from ec in Database.Microgen.EntityClients
                    join et in Database.Microgen.Entities on ec.EntityID equals et.EntityID
                    join er in Database.Microgen.EntityRoles on et.EntityID equals er.EntityID
                    where et.StatusID == 51004 && et.EntityTypeID == 2 && er.RoleTypeID == 1001 && et.EntityKey == petra_id
                    select new crmCompanyDetails() { petra_id = et.EntityKey, id = et.EntityID, company_name = ec.FullName, bus_reg_num = ec.RegisteredCompanyNum, contact_person = "", email = "", mobile_no = "", telephone_no = "" }).Single<crmCompanyDetails>();
        }

        private static string GetUserName(int id)
        {
            try
            {
                var s = (from u in Database.ERP.ERP_Users where u.id == id select u).Single();
                return String.Format("{0} {1}", s.first_name, s.last_name);
            }
            catch (System.InvalidOperationException)
            {
                return String.Format("{0} {1}", "John", "Doe");
            }
        }

        private static string GetDueDate(DateTime created, int sla)
        {
            return created.AddMinutes(sla).ToString("dd-MMM-yyyy hh:mm tt");
        }

        private static DateTime GetDueDateSorter(DateTime created, int sla)
        {
            return created.AddMinutes(sla); 
        }

        private static string GetFormattedDate(DateTime created)
        {
            return created.ToString("dd-MMM-yyyy hh:mm tt");
        }

        #endregion
    }

    #endregion

    #region CRM Objects

    public class crmViews
    {
    }

    public class crmSLAView
    {
        public int Id { get; set; }
        public string code { get; set; }
        public string Name { get; set; }
        public int Pre_escalate { get; set; }
        public int Escalated { get; set; }
        public bool Active { get; set; }

    }

    public class crmCorrespondenceView
    {
        public int Id { get; set; }
        public string code { get; set; }
        public string Name { get; set; }
        public string category { get; set; }
        public String description { get; set; }
        public int category_id { get; set; }
        public  bool active { get; set; }
    }

    public class crmSubCorrespondenceView
    {
        public int Id { get; set; }
        public string code { get; set; }
        public string Name { get; set; }
        public String correspondence { get; set; }
        public String SLA { get; set; }
        public String description { get; set; }
        public int correspondence_id { get; set; }
        public int sla_id { get; set; }
        public bool active { get; set; }
    }

    public class crmCategoryView
    {
        public int Id { get; set; }
        public string code { get; set; }
        public string Name { get; set; }
        public String description { get; set; }
        public bool active { get; set; }
    }

    public class crmTicketCounter
    {
        public int ini_serial { get; set; }
    }

    public class crmTicketsView
    {
        public string ticket_id { get; set; }
        public string subject { get; set; }
        public string category { get; set; }
        public string correspondence { get; set; }
        public string subcorrespondence { get; set; }
        public string Assigned_To { get; set; }
        public string created_at { get; set; }
        public string escalation_due { get; set; }
        public string status { get; set; }
    }

    public class crmTicketDetails
    {
        public int id { get; set; }
        public string ticket_id { get; set; }
        public string petra_id { get; set; }
        public string ticket_date { get; set; }
        public int esacalation { get; set; }
        public string subject { get; set; }
        public int customer_type { get; set; }
        public int status_id { get; set; }
        public string notes { get; set; }
        public string category { get; set; }
        public string correspondence { get; set; }
        public string subcorrespondence { get; set; }
        public string owner { get; set; }
        public int ownerid { get; set; }
        public int assigned_to { get; set; }
        public string contact_no { get; set; }
        public string email { get; set; }
    }

    public class crmTicketComments
    {
        public string comment_date { get; set; }
        public string comment { get; set; }
        public string owner { get; set; }
    }

    public class crmTicketStatus
    {
        public int id { get; set; }
        public string status { get; set; }
    }

    #endregion

    #region CRM Microgen Objects

    public class crmCustomerFullDetails
    {
        public int Entity_ID { get; set; }
        public string Petra_ID { get; set; }
        public string First_Name { get; set; }
        public string Second_Name { get; set; }
        public string Last_Name { get; set; }
        public string SSNIT_No { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Employer { get; set; }
    }

    public class crmCustomerDetails
    {
        public string Petra_ID { get; set; }
        public string Customer_Name { get; set; }
        public string SSNIT_No { get; set; }
        public string Employer { get; set; }
    }

    public class crmCustomerEmpoyerDetails
    {
        public string Employer_Name { get; set; }
    }

    public class crmCustomerContactNoDetails
    {
        public string Contact_No { get; set; }
    }

    public class crmCustomerEmailsDetails
    {
        public string Email { get; set; }
    }

    public class crmCustomerContactDetails
    {
        public string phone { set; get; }
        public string email { set; get; }
    }

    public class crmCompanyDetails
    {
        public int id { set; get; }
        public string petra_id { set; get; }
        public string company_name { set; get; }
        public string bus_reg_num { set; get; }
        public string contact_person { set; get; }
        public string mobile_no { set; get; }
        public string telephone_no { set; get; }
        public string email { set; get; }
    }

    public class crmCompanyList
    {
        public string petra_id { set; get; }
        public string company_name { set; get; }
    }

    #endregion
}
