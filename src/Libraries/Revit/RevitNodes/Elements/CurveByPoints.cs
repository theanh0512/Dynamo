﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using DSNodeServices;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit Curve By Points
    /// </summary>
    [RegisterForTrace]
    public class CurveByPoints : CurveElement
    {
        #region private mutators

        private void InternalSetReferencePoints(ReferencePointArray pts)
        {
            try
            {
                TransactionManager.GetInstance().EnsureInTransaction(Document);
                ((Autodesk.Revit.DB.CurveByPoints) InternalCurveElement).SetPoints(pts);
                TransactionManager.GetInstance().TransactionTaskDone();
            }
            catch (Exception ex)
            {
                TransactionManager.GetInstance().ForceCloseTransaction();
                throw new Exception("Unable to update curve by points. \n" + ex.Message);
            }
        }

        #endregion

        #region private constructors

        /// <summary>
        /// Construct a model curve from the document.  The result is Dynamo owned
        /// </summary>
        /// <param name="curve"></param>
        private CurveByPoints(Autodesk.Revit.DB.CurveByPoints curveByPoints)
        {
            InternalSetCurveElement(curveByPoints);
        }

        private CurveByPoints(IEnumerable<Autodesk.Revit.DB.ReferencePoint> refPoints)
        {
            //Add all of the elements in the sequence to a ReferencePointArray.
            var refPtArr = new ReferencePointArray();
            foreach (var refPt in refPoints)
            {
                refPtArr.Append(refPt);
            }

            //Phase 1 - Check to see if the object exists and should be rebound
            var cbp =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.CurveByPoints>(Document);

            if (cbp != null)
            {
                InternalSetCurveElement(cbp);
                InternalSetReferencePoints(refPtArr);
                return;
            }

            try
            {
                TransactionManager.GetInstance().EnsureInTransaction(Document);

                cbp = DocumentManager.GetInstance().CurrentDBDocument.FamilyCreate.NewCurveByPoints(refPtArr);

                cbp.IsReferenceLine = false;

                InternalSetCurveElement(cbp);

                TransactionManager.GetInstance().TransactionTaskDone();

                ElementBinder.SetElementForTrace(this.InternalElementId);
            }
            catch (Exception ex)
            {
                TransactionManager.GetInstance().ForceCloseTransaction();
                throw new Exception("Unable to create curve by points. \n" + ex.Message);
            }
        }

        #endregion

        #region public constructors

        /// <summary>
        /// Construct a Revit ModelCurve element from a Curve
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static CurveByPoints ByReferencePoints(IEnumerable<ReferencePoint> points)
        {
            if (points.Count() < 2)
            {
                throw new Exception("Cannot create Curve By Points with less than two points.");
            }

            return new CurveByPoints(points.Select(x=>x.InternalReferencePoint));
        }

        /// <summary>
        /// Construct a Revit ModelCurve element from an existing element.  The result is Dynamo owned.
        /// </summary>
        /// <param name="modelCurve"></param>
        /// <returns></returns>
        internal static CurveByPoints FromExisting(Autodesk.Revit.DB.CurveByPoints curveByPoints, bool isRevitOwned)
        {
            return new CurveByPoints(curveByPoints)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    }
}