﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SensorWeb.Models;
using SelectPdf;
using System.Threading.Tasks;
using Core.Service;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Core;

namespace SensorWeb.Controllers
{
    [Authorize]
    public class ReportController : BaseController
    {
        IMapper _mapper;
        private readonly IStringLocalizer<Resources.CommonResources> _localizer;
        IReceiveService _receiveService;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="localizer"></param>
        public ReportController(IMapper mapper, IStringLocalizer<Resources.CommonResources> localizer, IReceiveService ReceiveService)
        {
            _mapper = mapper;
            _localizer = localizer;
            _receiveService = ReceiveService;
        }

        // GET: ReportController
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReportModel report)
        {
            var reportView = await this.RenderViewToStringAsync("DownloadPDF", report);

            // instantiate a html to pdf converter object
            HtmlToPdf converter = new HtmlToPdf();


            converter.Options.CssMediaType = HtmlToPdfCssMediaType.Print;
            //converter.Options.ViewerPreferences.FitWindow = true;
            converter.Options.ViewerPreferences.CenterWindow = true;
            converter.Options.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
            converter.Options.AutoFitWidth = HtmlToPdfPageFitMode.AutoFit;
            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

            PdfDocument doc = converter.ConvertHtmlString(reportView);

            byte[] pdf = doc.Save();
            doc.Close();

            FileResult fileResult = new FileContentResult(pdf, "application/pdf")
            {
                FileDownloadName = $"{report.NomeArquivo}.pdf"
            };

            return fileResult;
        }

        public ActionResult ReportDeviceData(int? DeviceId, int? MotorId)
        {
            ViewBag.DeviceData = new List<SelectListItem>();

            if (DeviceId != null && MotorId != null)
            {
                var dataList = _receiveService.GetDataByDeviceMotor(DeviceId, MotorId);
                if (dataList != null)
                {
                    ViewBag.DeviceData = dataList.Select(d => new SelectListItem
                    {
                        Text = string.Format("{0} - {1}", d.DataReceive.ToString("dd/MM/yyyy HH:mm:ss"), d.id),
                        Value = d.IdReceiveData.ToString(),
                    });
                }
            }
                
            return View();
        }

        public JsonResult ReportDeviceDataUpdate(int idDataReceive)
        {
            var dadosDataReceive = _receiveService.GetDataDadoByDataReceiveId(idDataReceive);

            return Json(dadosDataReceive);
        }
    }
}
