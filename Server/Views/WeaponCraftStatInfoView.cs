﻿using System;
using DevExpress.XtraBars;
using Library;
using Library.SystemModels;

namespace Server.Views
{
    public partial class WeaponCraftStatInfoView : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public WeaponCraftStatInfoView()
        {
            InitializeComponent();

            ItemInfoStatGridControl.DataSource = SMain.Session.GetCollection<WeaponCraftStatInfo>().Binding;
            
            StatImageComboBox.Items.AddEnum<Stat>();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SMain.SetUpView(ItemInfoStatGridView);
        }

        private void SaveButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            SMain.Session.Save(true);
        }

        private void ImportButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            JsonImporter.Import<WeaponCraftStatInfo>();
        }

        private void ExportButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            JsonExporter.Export<WeaponCraftStatInfo>(ItemInfoStatGridView);
        }
    }
}