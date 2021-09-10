﻿using Models;
using Models.Core;
using Models.Factorial;
using Models.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserInterface.Interfaces;
using UserInterface.Presenters;
using UserInterface.Views;

namespace UserInterface.Presenters
{
    class CLEMReportResultsPresenter : IPresenter, ICLEMPresenter, IRefreshPresenter
    {
        /// <summary>
        /// The data storage
        /// </summary>
        private IDataStore dataStore;

        /// <summary>
        /// The CLEM view
        /// </summary>
        private CLEMView clem;

        /// <summary>
        /// Attach the model and view to the presenter
        /// </summary>
        /// <param name="model">The model to attach</param>
        /// <param name="view">The view to attach</param>
        /// <param name="explorerPresenter">The presenter to attach to</param>
        public void Attach(object model, object view, ExplorerPresenter explorerPresenter)
        {
            // This code is not reached, the usual functionality is performed in
            // the CLEMPresenter.AttachExtraPresenters() method
        }

        /// <inheritdoc/>
        public void AttachExtraPresenters(CLEMPresenter clemPresenter)
        {
            try
            {
                bool parentOfReport = false;
                Report report = clemPresenter.model as Report;
                if(report is null)
                {
                    report = (clemPresenter.model as Model).FindChild<Report>();
                    parentOfReport = true;
                }

                ReportView rv = new ReportView(clemPresenter.view as ViewBase);
                ViewBase reportView = new ViewBase(rv, "ApsimNG.Resources.Glade.DataStoreView.glade");

                DataStorePresenter dataStorePresenter = new DataStorePresenter(new string[] { (parentOfReport)? (clemPresenter.model as IModel).Name:report.Name });

                Simulations simulations = report.FindAncestor<Simulations>();
                if (simulations != null)
                    dataStore = simulations.FindChild<IDataStore>();

                Simulation simulation = report.FindAncestor<Simulation>();
                Experiment experiment = report.FindAncestor<Experiment>();
                Zone paddock = report.FindAncestor<Zone>();

                IModel zoneAnscestor = report.FindAncestor<Zone>();

                // Only show data which is in scope of this report.
                // E.g. data from this zone and either experiment (if applicable) or simulation.
                if (paddock != null)
                    dataStorePresenter.ZoneFilter = paddock;
                if (zoneAnscestor is null & experiment != null)
                    // allows the inner reports of the base simulation to be displayed
                    // when an experiment is being undertaken
                    // otherwise reports are considered child of experiment and will only display experiment results.
                    dataStorePresenter.ExperimentFilter = experiment;
                else if (simulation != null)
                    dataStorePresenter.SimulationFilter = simulation;

                dataStorePresenter.Attach(dataStore, reportView, clemPresenter.explorerPresenter);

                // Attach the view to display data
                clem = clemPresenter.view as CLEMView;
                clem.AddTabView("Data", reportView);
                clemPresenter.presenterList.Add("Data", this);
            }
            catch (Exception err)
            {
                clemPresenter.explorerPresenter.MainPresenter.ShowError(err);
            }
        }

        /// <inheritdoc/>
        public void Detach()
        { }

        /// <inheritdoc/>
        public void Refresh() { } 
    }
}
