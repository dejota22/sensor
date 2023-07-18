﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AutoMapper;
using Core;
using Core.ApiModel.Request;
using Core.DTO;
using SensorApi.Models;
using Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SensorApi.Interfaces;
using Newtonsoft.Json;
using Core.ApiModel.Response;
using SensorService;
using Org.BouncyCastle.Crypto.Agreement;
using Autofac.Core;

namespace SensorApi.Controllers
{
    [Authorize]
    [Route("api/device")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceConfigurationService _DeviceConfigurationService;
        private readonly IDeviceMeasureService _DeviceMeasureService;
        private readonly IMotorService _MotorService;
        private readonly IDeviceService _DeviceService;
        private readonly IReceiveService _ReceiveService;
        private readonly IJwtAuth _jwtAuth;
        private readonly IMapper _mapper;


        private readonly IMachineService _MachineService;
        private readonly IFixationTypeService _FixationService;
        private readonly ICouplingTypeService _CouplingService;
        private readonly ICardanShaftTypeService _CardanShaftTypeService;
        private readonly IPumpTypeService _PumpTypeService;
        private readonly ICompressorTypeService _CompressorTypeService;
        //private readonly ILogger<DeviceController> _logger;

        /// <summary>
        /// Construtor
        /// </summary>        
        public DeviceController(IDeviceConfigurationService DeviceConfigurationService, IDeviceMeasureService DeviceMeasureService, IMotorService MotorService, IDeviceService DeviceService, IReceiveService ReceiveService, IJwtAuth jwtAuth, IMapper mapper,
            IMachineService MachineService, IFixationTypeService FixationService, ICouplingTypeService CouplingService,
            ICardanShaftTypeService CardanShaftTypeService, IPumpTypeService PumpTypeService, ICompressorTypeService CompressorTypeService)
        {
            _DeviceConfigurationService = DeviceConfigurationService;
            _DeviceMeasureService = DeviceMeasureService;
            _MotorService = MotorService;
            _DeviceService = DeviceService;
            _ReceiveService = ReceiveService;
            _jwtAuth = jwtAuth;
            _mapper = mapper;
            _MachineService = MachineService;
            _FixationService = FixationService;
            _CouplingService = CouplingService;
            _CardanShaftTypeService = CardanShaftTypeService;
            _PumpTypeService = PumpTypeService;
            _CompressorTypeService = CompressorTypeService;
            //  _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        [Produces("application/json")]
        public ContentResult Get()
        {

            //_logger.LogInformation($"Acesso ok");

            string respJson = "{\"Status\":\"Active\",\"Connection\":\"Ok\"}";
            ContentResult contRes1 = Content(Newtonsoft.Json.Linq.JObject.Parse(respJson).ToString(), "application/json");
            return contRes1;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("deviceMeasure")]
        public ContentResult AddDeviceMeasure([FromBody] DeviceRequest devicemeasure)
        {
            string respJson = string.Empty;

            if (devicemeasure == null)
            {
                respJson = "{\"Erro Interno\":\"Erro na leitura dos dados recebidos - Estrutura vazia\"}";
                ContentResult contRes1 = Content(Newtonsoft.Json.Linq.JObject.Parse(respJson).ToString(), "application/json");
                contRes1.StatusCode = (int)HttpStatusCode.InternalServerError;
                return contRes1;
            }

            ////Faz busca do Motor para salvar informaões no Measure
            ////var getDevice = _MotorService.GetBymotorTag(devicemeasure.T);

            //var getDevice = _DeviceService.GetByDeviceTag(devicemeasure.T);
            //if (getDevice == null)
            //{
            //    respJson = "{\"Erro\":\"Tag do Sensor não encontrado no cadastro\"}";
            //    ContentResult contRes1 = Content(Newtonsoft.Json.Linq.JObject.Parse(respJson).ToString(), "application/json");
            //    contRes1.StatusCode = (int)HttpStatusCode.InternalServerError;
            //    return contRes1;
            //}

            //DeviceMeasure deviMes = new DeviceMeasure
            //{
            //    //MotorId = getDevice.Id,
            //    DeviceId = getDevice.Id,
            //    Temperature = devicemeasure.E, //Temperature
            //    ReadDataType = devicemeasure.R, //Tipo de Leitura 
            //    CreatedAt = DateTime.Now,
            //    YAxle = string.Empty,
            //    ZAxle = string.Empty,
            //    XAxle = string.Empty,
            //};

            //try
            //{
            //    deviMes.XAxle = devicemeasure.D.Where(x => !String.IsNullOrEmpty(x.x)).FirstOrDefault().x; //Eixo x
            //}
            //catch { }
            //try
            //{
            //    deviMes.YAxle = devicemeasure.D.Where(y => !String.IsNullOrEmpty(y.y)).FirstOrDefault().y; //Eixo y
            //}
            //catch { }
            //try
            //{
            //    deviMes.ZAxle = devicemeasure.D.Where(z => !String.IsNullOrEmpty(z.z)).FirstOrDefault().z; //Eixo z
            //}
            //catch { }

            //_DeviceMeasureService.Insert(deviMes);

            respJson = "{\"T\":\"13b579\",\"R\":\"S6302$\",\"S\":\"14400\"}";
            ContentResult contRes = Content(Newtonsoft.Json.Linq.JObject.Parse(respJson).ToString(), "application/json");
            contRes.ContentType = "application/json";
            return contRes;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("deviceGlobal")]
        public ContentResult AddDeviceGlobal([FromBody] DeviceGlobalRequest deviceGlobal)
        {
            string respJson;
            if (deviceGlobal == null)
            {
                respJson = "{\"Erro Interno\":\"Erro na leitura dos dados recebidos - Estrutura vazia\"}";
                ContentResult contRes1 = Content(Newtonsoft.Json.Linq.JObject.Parse(respJson).ToString(), "application/json");
                contRes1.StatusCode = (int)HttpStatusCode.InternalServerError;
                return contRes1;
            }

            try
            {
                var path = "C:\\Log\\global.txt";
                var text = String.Format("{0}: {1}", DateTime.Now, JsonConvert.SerializeObject(deviceGlobal));
                if (!System.IO.File.Exists(path))
                {
                    var createFile = System.IO.File.Create(path);
                    createFile.Close();
                }

                System.IO.File.AppendAllText(path, text + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            var deviceConfigEnviados = new List<int>();
            var response = new DeviceGlobalResponse() { };
            var defaultLora = _DeviceConfigurationService.GetLast(0, 0);

            var deviceConfigList = _DeviceConfigurationService.GetAll()
                .GroupBy(s => s.DeviceId);

            response.Sensor = new List<Sensor>();
            response.Horarios = new List<Horario>();
            response.Gatilhos = new List<Gatilho>();
            response.Lora = new List<Lora>();

            foreach (var deviceConfig in deviceConfigList)
            {
                var deviceConfigId = 0;
                var sensor = new Sensor();
                sensor.SetupAcc = new List<Setup>();
                sensor.SetupSpd = new List<Setup>();
                sensor.SetupUsr = new List<Setup>();

                var horario = new Horario();
                horario.HorariosEnviosCard = new List<HorariosEnviosCard>();

                var gatilho = new Gatilho();
                var lora = new Lora();

                foreach (var setup in deviceConfig)
                {
                    deviceConfigId = setup.Id;

                    sensor.Config = Convert.ToInt32(setup.config);

                    sensor.SetupAcc.Add(new Setup()
                    {
                        amostras = setup.acc_amostras.HasValue ? setup.acc_amostras.Value : 0,
                        eixo = setup.acc_eixo.HasValue ? setup.acc_eixo.Value : 0,
                        filtro = setup.acc_filtro.HasValue ? setup.acc_filtro.Value : 0,
                        freq_cut = setup.acc_freq_cut.HasValue ? setup.acc_freq_cut.Value : 0,
                        fs = setup.acc_fs.HasValue ? setup.acc_fs.Value : 0,
                        Odr = setup.acc_odr.HasValue ? setup.acc_odr.Value : 0
                    });

                    sensor.SetupSpd.Add(new Setup()
                    {
                        amostras = setup.spd_amostras.HasValue ? setup.spd_amostras.Value : 0,
                        eixo = setup.spd_eixo.HasValue ? setup.spd_eixo.Value : 0,
                        filtro = setup.spd_filtro.HasValue ? setup.spd_filtro.Value : 0,
                        freq_cut = setup.spd_freq_cut.HasValue ? setup.spd_freq_cut.Value : 0,
                        fs = setup.spd_fs.HasValue ? setup.spd_fs.Value : 0,
                        Odr = setup.spd_odr.HasValue ? setup.spd_odr.Value : 0
                    });

                    var usrSetupData = _DeviceConfigurationService.GetUsrSetup(setup.MotorId.Value, setup.DeviceId.Value);
                    if (usrSetupData != null)
                    {
                        sensor.SetupUsr.Add(new Setup()
                        {
                            amostras = usrSetupData.usr_amostras.HasValue ? usrSetupData.usr_amostras.Value : 0,
                            eixo = usrSetupData.usr_eixo.HasValue ? usrSetupData.usr_eixo.Value : 0,
                            filtro = usrSetupData.usr_filtro.HasValue ? usrSetupData.usr_filtro.Value : 0,
                            freq_cut = usrSetupData.usr_freq_cut.HasValue ? usrSetupData.usr_freq_cut.Value : 0,
                            fs = usrSetupData.usr_fs.HasValue ? usrSetupData.usr_fs.Value : 0,
                            Odr = usrSetupData.usr_odr.HasValue ? usrSetupData.usr_odr.Value : 0
                        });
                    }

                    horario.Config = Convert.ToInt32(setup.config);
                    horario.QuantHorariosCards = setup.quant_horarios_cards.HasValue ? setup.quant_horarios_cards.Value : 0;
                    horario.ModoHora = setup.modo_hora.HasValue ? setup.modo_hora.Value : 0;
                    horario.IntervaloAnalise = setup.intervalo_analise.HasValue ? setup.intervalo_analise.Value : 0;
                    horario.IntervaloAnaliseAlarme = setup.intervalo_analise_alarme.HasValue ? setup.intervalo_analise_alarme.Value : 0;
                    horario.ContaEnvios = setup.conta_envios.HasValue ? setup.conta_envios.Value : 0;
                    horario.DiaEnvioRelat = setup.dia_envio_relat;
                    horario.HoraEnvioRelat = setup.hora_envio_relat;
                    horario.DiasRun = setup.dias_run;
                    horario.FimTurno = setup.fim_turno;
                    horario.InicioTurno = setup.inicio_turno;
                    horario.t_card_normal = setup.t_card_normal.HasValue ? setup.t_card_normal.Value : 0;

                    horario.HorariosEnviosCard = new List<HorariosEnviosCard>();
                    setup.DeviceConfigurationHorariosEnviosCard = _DeviceConfigurationService.GetHoras(setup.Id);
                    foreach (var hora in setup.DeviceConfigurationHorariosEnviosCard)
                    {
                        horario.HorariosEnviosCard.Add(new HorariosEnviosCard()
                        {
                            Hora = hora.Hora != null ? hora.Hora.Substring(0, 5) : ""
                        });
                    }

                    gatilho.Config = Convert.ToInt32(setup.config);
                    gatilho.RmsAccRed = setup.rms_acc_red.HasValue ?
                        Decimal.ToDouble(setup.rms_acc_red.Value) : 0;
                    gatilho.RmsAccYel = setup.rms_acc_yel.HasValue ?
                        Decimal.ToDouble(setup.rms_acc_yel.Value) : 0;
                    gatilho.MinRmsAcc = setup.min_rms_acc.HasValue ?
                        Decimal.ToDouble(setup.min_rms_acc.Value) : 0;
                    gatilho.RmsSpdRed = setup.rms_spd_red.HasValue ?
                        Decimal.ToDouble(setup.rms_spd_red.Value) : 0;
                    gatilho.RmsSpdYel = setup.rms_spd_yel.HasValue ?
                        Decimal.ToDouble(setup.rms_spd_yel.Value) : 0;
                    gatilho.MaxVar = setup.max_var.HasValue ? setup.max_var.Value : 0;

                    var loraSetupData = defaultLora;
                    lora.Canal = loraSetupData.canal.Value;
                    lora.End = loraSetupData.end.Value;
                    lora.Syn = loraSetupData.syn.Value;
                    lora.Sf = loraSetupData.sf.Value;
                    lora.Bw = loraSetupData.bw.Value;
                    lora.Gtw = loraSetupData.gtw.Value;
                }

                var device = _DeviceService.Get(deviceConfig.Key.Value);
                if (device != null && deviceGlobal.Id == device.Code)
                {
                    response.Sensor.Add(sensor);
                    response.Horarios.Add(horario);
                    response.Gatilhos.Add(gatilho);
                    response.Lora.Add(lora);
                    response.Versao = "1.3.1";
                    response.Sn = device.Code;

                    deviceConfigEnviados.Add(deviceConfigId);

                    var receiveGlobal = GenerateEntityFromDeviceGlobalRequest(deviceGlobal, deviceConfigId);
                    _ReceiveService.InsertGlobal(receiveGlobal);
                }
            }

            if (deviceConfigEnviados.Any() == false)
            {
                Lora lora = new Lora();
                lora.Canal = defaultLora.canal.Value;
                lora.End = defaultLora.end.Value;
                lora.Syn = defaultLora.syn.Value;
                lora.Sf = defaultLora.sf.Value;
                lora.Bw = defaultLora.bw.Value;
                lora.Gtw = defaultLora.gtw.Value;

                AddEmptySensorToResponse(response, lora, deviceGlobal.Id);
            }

            respJson = JsonConvert.SerializeObject(response);

            ContentResult contRes = Content(Newtonsoft.Json.Linq.JObject.Parse(respJson).ToString(), "application/json");
            contRes.StatusCode = (int)HttpStatusCode.OK;

            foreach (var dcId in deviceConfigEnviados)
            {
                var dc = _DeviceConfigurationService.Get(dcId);
                if (dc != null)
                {
                    dc.config = false;
                    _DeviceConfigurationService.Edit(dc, null);
                }
            }

            return contRes;
        }

        private ReceiveGlobal GenerateEntityFromDeviceGlobalRequest(DeviceGlobalRequest deviceGlobal, int idDeviceConfiguration)
        {
            return new ReceiveGlobal()
            {
                id = deviceGlobal.Id,
                IdDeviceConfiguration = idDeviceConfiguration,
                gtw = deviceGlobal.gtw,
                ver = deviceGlobal.versao,
                seq = deviceGlobal.seq,
                resets = deviceGlobal.resets,
                card_lidos = deviceGlobal.leituras_efetuadas,
                card_send = deviceGlobal.q_cards_enviados,
                relat_send = deviceGlobal.q_relats_enviados,
                relat_erros = deviceGlobal.relat_erros,
                freq = deviceGlobal.FreqFine,
                temp = deviceGlobal.temp,
                alrm = deviceGlobal.alarme,
                rms_acc_X = deviceGlobal.rms_acc_X,
                rms_acc_Y = deviceGlobal.rms_acc_Y,
                rms_acc_Z = deviceGlobal.rms_acc_Z,
                rms_spd_X = deviceGlobal.rms_spd_X,
                rms_spd_Y = deviceGlobal.rms_spd_Y,
                rms_spd_Z = deviceGlobal.rms_spd_Z,
                setup_af_amostras = deviceGlobal.setup_spd != null && deviceGlobal.setup_spd.Any() ?
                    deviceGlobal.setup_spd.First().amostras : 0,
                setup_af_filtro = deviceGlobal.setup_spd != null && deviceGlobal.setup_spd.Any() ?
                    deviceGlobal.setup_spd.First().filtro : 0,
                setup_af_freq_cut = deviceGlobal.setup_spd != null && deviceGlobal.setup_spd.Any() ?
                    deviceGlobal.setup_spd.First().freq_cut : 0,
                setup_af_fs = deviceGlobal.setup_spd != null && deviceGlobal.setup_spd.Any() ?
                    deviceGlobal.setup_spd.First().fs : 0,
                setup_af_odr = deviceGlobal.setup_spd != null && deviceGlobal.setup_spd.Any() ?
                    deviceGlobal.setup_spd.First().odr : 0,
                setup_bf_amostras = deviceGlobal.setup_acc != null && deviceGlobal.setup_acc.Any() ?
                    deviceGlobal.setup_acc.First().amostras : 0,
                setup_bf_filtro = deviceGlobal.setup_acc != null && deviceGlobal.setup_acc.Any() ?
                    deviceGlobal.setup_acc.First().filtro : 0,
                setup_bf_freq_cut = deviceGlobal.setup_acc != null && deviceGlobal.setup_acc.Any() ?
                    deviceGlobal.setup_acc.First().freq_cut : 0,
                setup_bf_fs = deviceGlobal.setup_acc != null && deviceGlobal.setup_acc.Any() ?
                    deviceGlobal.setup_acc.First().fs : 0,
                setup_bf_odr = deviceGlobal.setup_acc != null && deviceGlobal.setup_acc.Any() ?
                    deviceGlobal.setup_acc.First().odr : 0
            };
        }

        [HttpGet]
        [Route("EnviaConfigSensor")]
        public ContentResult EnviaConfigSensor(string id)
        {
            SensorData sdata = new SensorData() { id = id };
            return EnviaConfigSensor(sdata);
        }

        [HttpGet]
        [Route("EnviaConfigSensorJson")]
        public ContentResult EnviaConfigSensor([FromBody] SensorData sdata)
        {
            string id = sdata.id;
            string respJson;

            var deviceConfigEnviados = new List<int>();
            var response = new DeviceGlobalResponse() { };
            var defaultLora = _DeviceConfigurationService.GetLast(0, 0);

            var deviceConfigList = _DeviceConfigurationService.GetAll()
                .GroupBy(s => s.DeviceId);

            response.Sensor = new List<Sensor>();
            response.Horarios = new List<Horario>();
            response.Gatilhos = new List<Gatilho>();
            response.Lora = new List<Lora>();

            foreach (var deviceConfig in deviceConfigList)
            {
                var deviceConfigId = 0;
                var sensor = new Sensor();
                sensor.SetupAcc = new List<Setup>();
                sensor.SetupSpd = new List<Setup>();
                sensor.SetupUsr = new List<Setup>();

                var horario = new Horario();
                horario.HorariosEnviosCard = new List<HorariosEnviosCard>();

                var gatilho = new Gatilho();
                var lora = new Lora();

                foreach (var setup in deviceConfig)
                {
                    deviceConfigId = setup.Id;

                    sensor.Config = Convert.ToInt32(setup.config);

                    sensor.SetupAcc.Add(new Setup()
                    {
                        amostras = setup.acc_amostras.HasValue ? setup.acc_amostras.Value : 0,
                        eixo = setup.acc_eixo.HasValue ? setup.acc_eixo.Value : 0,
                        filtro = setup.acc_filtro.HasValue ? setup.acc_filtro.Value : 0,
                        freq_cut = setup.acc_freq_cut.HasValue ? setup.acc_freq_cut.Value : 0,
                        fs = setup.acc_fs.HasValue ? setup.acc_fs.Value : 0,
                        Odr = setup.acc_odr.HasValue ? setup.acc_odr.Value : 0
                    });

                    sensor.SetupSpd.Add(new Setup()
                    {
                        amostras = setup.spd_amostras.HasValue ? setup.spd_amostras.Value : 0,
                        eixo = setup.spd_eixo.HasValue ? setup.spd_eixo.Value : 0,
                        filtro = setup.spd_filtro.HasValue ? setup.spd_filtro.Value : 0,
                        freq_cut = setup.spd_freq_cut.HasValue ? setup.spd_freq_cut.Value : 0,
                        fs = setup.spd_fs.HasValue ? setup.spd_fs.Value : 0,
                        Odr = setup.spd_odr.HasValue ? setup.spd_odr.Value : 0
                    });

                    var usrSetupData = _DeviceConfigurationService.GetUsrSetup(setup.MotorId.Value, setup.DeviceId.Value);
                    if (usrSetupData != null)
                    {
                        sensor.SetupUsr.Add(new Setup()
                        {
                            amostras = usrSetupData.usr_amostras.HasValue ? usrSetupData.usr_amostras.Value : 0,
                            eixo = usrSetupData.usr_eixo.HasValue ? usrSetupData.usr_eixo.Value : 0,
                            filtro = usrSetupData.usr_filtro.HasValue ? usrSetupData.usr_filtro.Value : 0,
                            freq_cut = usrSetupData.usr_freq_cut.HasValue ? usrSetupData.usr_freq_cut.Value : 0,
                            fs = usrSetupData.usr_fs.HasValue ? usrSetupData.usr_fs.Value : 0,
                            Odr = usrSetupData.usr_odr.HasValue ? usrSetupData.usr_odr.Value : 0
                        });
                    }

                    horario.Config = Convert.ToInt32(setup.config);
                    horario.QuantHorariosCards = setup.quant_horarios_cards.HasValue ? setup.quant_horarios_cards.Value : 0;
                    horario.ModoHora = setup.modo_hora.HasValue ? setup.modo_hora.Value : 0;
                    horario.IntervaloAnalise = setup.intervalo_analise.HasValue ? setup.intervalo_analise.Value : 0;
                    horario.IntervaloAnaliseAlarme = setup.intervalo_analise_alarme.HasValue ? setup.intervalo_analise_alarme.Value : 0;
                    horario.ContaEnvios = setup.conta_envios.HasValue ? setup.conta_envios.Value : 0;
                    horario.DiaEnvioRelat = setup.dia_envio_relat;
                    horario.HoraEnvioRelat = setup.hora_envio_relat;
                    horario.DiasRun = setup.dias_run;
                    horario.FimTurno = setup.fim_turno;
                    horario.InicioTurno = setup.inicio_turno;
                    horario.t_card_normal = setup.t_card_normal.HasValue ? setup.t_card_normal.Value : 0;

                    horario.HorariosEnviosCard = new List<HorariosEnviosCard>();
                    setup.DeviceConfigurationHorariosEnviosCard = _DeviceConfigurationService.GetHoras(setup.Id);
                    foreach (var hora in setup.DeviceConfigurationHorariosEnviosCard)
                    {
                        horario.HorariosEnviosCard.Add(new HorariosEnviosCard()
                        {
                            Hora = hora.Hora != null ? hora.Hora.Substring(0, 5) : ""
                        });
                    }

                    gatilho.Config = Convert.ToInt32(setup.config);
                    gatilho.RmsAccRed = setup.rms_acc_red.HasValue ?
                        Decimal.ToDouble(setup.rms_acc_red.Value) : 0;
                    gatilho.RmsAccYel = setup.rms_acc_yel.HasValue ?
                        Decimal.ToDouble(setup.rms_acc_yel.Value) : 0;
                    gatilho.MinRmsAcc = setup.min_rms_acc.HasValue ?
                        Decimal.ToDouble(setup.min_rms_acc.Value) : 0;
                    gatilho.RmsSpdRed = setup.rms_spd_red.HasValue ?
                        Decimal.ToDouble(setup.rms_spd_red.Value) : 0;
                    gatilho.RmsSpdYel = setup.rms_spd_yel.HasValue ?
                        Decimal.ToDouble(setup.rms_spd_yel.Value) : 0;
                    gatilho.MaxVar = setup.max_var.HasValue ? setup.max_var.Value : 0;

                    var loraSetupData = defaultLora;
                    lora.Canal = loraSetupData.canal.Value;
                    lora.End = loraSetupData.end.Value;
                    lora.Syn = loraSetupData.syn.Value;
                    lora.Sf = loraSetupData.sf.Value;
                    lora.Bw = loraSetupData.bw.Value;
                    lora.Gtw = loraSetupData.gtw.Value;
                }

                var device = _DeviceService.Get(deviceConfig.Key.Value);
                if (device != null && id == device.Code)
                {
                    response.Sensor.Add(sensor);
                    response.Horarios.Add(horario);
                    response.Gatilhos.Add(gatilho);
                    response.Lora.Add(lora);
                    response.Versao = "1.3.1";
                    response.Sn = device.Code;

                    deviceConfigEnviados.Add(deviceConfigId);
                }
            }

            if (deviceConfigEnviados.Any() == false)
            {
                Lora lora = new Lora();
                lora.Canal = defaultLora.canal.Value;
                lora.End = defaultLora.end.Value;
                lora.Syn = defaultLora.syn.Value;
                lora.Sf = defaultLora.sf.Value;
                lora.Bw = defaultLora.bw.Value;
                lora.Gtw = defaultLora.gtw.Value;

                AddEmptySensorToResponse(response, lora, id);
            }

            respJson = JsonConvert.SerializeObject(response);

            ContentResult contRes = Content(Newtonsoft.Json.Linq.JObject.Parse(respJson).ToString(), "application/json");
            contRes.StatusCode = (int)HttpStatusCode.OK;

            foreach (var dcId in deviceConfigEnviados)
            {
                var dc = _DeviceConfigurationService.Get(dcId);
                if (dc != null)
                {
                    dc.config = false;
                    _DeviceConfigurationService.Edit(dc, null);
                }
            }

            return contRes;
        }

        private void AddEmptySensorToResponse(DeviceGlobalResponse response, Lora lora, string deviceGlobalId)
        {
            Sensor sensor = new Sensor()
            {
                Config = 0,
                SetupAcc = new List<Setup>()
                {
                    new Setup()
                    {
                        amostras = 0, eixo = 0, filtro = 0, freq_cut = 0, fs = 0, Odr = 0
                    }
                },
                SetupSpd = new List<Setup>()
                {
                    new Setup()
                    {
                        amostras = 0, eixo = 0, filtro = 0, freq_cut = 0, fs = 0, Odr = 0
                    }
                },
                SetupUsr = new List<Setup>()
            };

            Horario horario = new Horario()
            {
                Config = 0, ContaEnvios = 0, DiaEnvioRelat = "________", DiasRun = "________", FimTurno = "00:00", HoraEnvioRelat = "00:00",
                InicioTurno = "00:00", IntervaloAnalise = 0, IntervaloAnaliseAlarme = 0, ModoHora = 0, QuantAlarme = 0, QuantHorariosCards = 0,
                HorariosEnviosCard = new List<HorariosEnviosCard>()
            };

            Gatilho gatilho = new Gatilho()
            {
                Config = 0, MaxVar = 0, RmsAccRed = 0, RmsAccYel = 0, MinRmsAcc = 0
            };

            response.Sensor.Add(sensor);
            response.Horarios.Add(horario);
            response.Gatilhos.Add(gatilho);
            response.Lora.Add(lora);
            response.Versao = "1.3.1";
            response.Sn = deviceGlobalId;
        }

        [HttpPost]
        [Route("deviceData")]
        public ContentResult AddDeviceData([FromBody] DeviceDataRequest deviceData)
        {
            string respJson;
            if (deviceData == null)
            {
                respJson = "{\"Erro Interno\":\"Erro na leitura dos dados recebidos - Estrutura vazia\"}";
                ContentResult contRes1 = Content(Newtonsoft.Json.Linq.JObject.Parse(respJson).ToString(), "application/json");
                contRes1.StatusCode = (int)HttpStatusCode.InternalServerError;
                return contRes1;
            }

            try
            {
                var path = "C:\\Log\\data.txt";
                var text = String.Format("{0}: {1}", DateTime.Now, JsonConvert.SerializeObject(deviceData));

                if (!System.IO.File.Exists(path))
                {
                    var createFile = System.IO.File.Create(path);
                    createFile.Close();
                }

                System.IO.File.AppendAllText(path, text + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


            var deviceConfigEnviados = new List<int>();
            var response = new DeviceGlobalResponse() { };
            var defaultLora = _DeviceConfigurationService.GetLast(0, 0);

            var deviceConfigList = _DeviceConfigurationService.GetAll()
                .GroupBy(s => s.DeviceId);

            response.Sensor = new List<Sensor>();
            response.Horarios = new List<Horario>();
            response.Gatilhos = new List<Gatilho>();
            response.Lora = new List<Lora>();

            foreach (var deviceConfig in deviceConfigList)
            {
                var deviceConfigId = 0;
                var sensor = new Sensor();
                sensor.SetupAcc = new List<Setup>();
                sensor.SetupSpd = new List<Setup>();
                sensor.SetupUsr = new List<Setup>();

                var horario = new Horario();
                horario.HorariosEnviosCard = new List<HorariosEnviosCard>();

                var gatilho = new Gatilho();
                var lora = new Lora();

                foreach (var setup in deviceConfig)
                {
                    deviceConfigId = setup.Id;

                    sensor.Config = Convert.ToInt32(setup.config);

                    sensor.SetupAcc.Add(new Setup()
                    {
                        amostras = setup.acc_amostras.HasValue ? setup.acc_amostras.Value : 0,
                        eixo = setup.acc_eixo.HasValue ? setup.acc_eixo.Value : 0,
                        filtro = setup.acc_filtro.HasValue ? setup.acc_filtro.Value : 0,
                        freq_cut = setup.acc_freq_cut.HasValue ? setup.acc_freq_cut.Value : 0,
                        fs = setup.acc_fs.HasValue ? setup.acc_fs.Value : 0,
                        Odr = setup.acc_odr.HasValue ? setup.acc_odr.Value : 0
                    });

                    sensor.SetupSpd.Add(new Setup()
                    {
                        amostras = setup.spd_amostras.HasValue ? setup.spd_amostras.Value : 0,
                        eixo = setup.spd_eixo.HasValue ? setup.spd_eixo.Value : 0,
                        filtro = setup.spd_filtro.HasValue ? setup.spd_filtro.Value : 0,
                        freq_cut = setup.spd_freq_cut.HasValue ? setup.spd_freq_cut.Value : 0,
                        fs = setup.spd_fs.HasValue ? setup.spd_fs.Value : 0,
                        Odr = setup.spd_odr.HasValue ? setup.spd_odr.Value : 0
                    });

                    var usrSetupData = _DeviceConfigurationService.GetUsrSetup(setup.MotorId.Value, setup.DeviceId.Value);
                    if (usrSetupData != null)
                    {
                        sensor.SetupUsr.Add(new Setup()
                        {
                            amostras = usrSetupData.usr_amostras.HasValue ? usrSetupData.usr_amostras.Value : 0,
                            eixo = usrSetupData.usr_eixo.HasValue ? usrSetupData.usr_eixo.Value : 0,
                            filtro = usrSetupData.usr_filtro.HasValue ? usrSetupData.usr_filtro.Value : 0,
                            freq_cut = usrSetupData.usr_freq_cut.HasValue ? usrSetupData.usr_freq_cut.Value : 0,
                            fs = usrSetupData.usr_fs.HasValue ? usrSetupData.usr_fs.Value : 0,
                            Odr = usrSetupData.usr_odr.HasValue ? usrSetupData.usr_odr.Value : 0
                        });
                    }

                    horario.Config = Convert.ToInt32(setup.config);
                    horario.QuantHorariosCards = setup.quant_horarios_cards.HasValue ? setup.quant_horarios_cards.Value : 0;
                    horario.ModoHora = setup.modo_hora.HasValue ? setup.modo_hora.Value : 0;
                    horario.IntervaloAnalise = setup.intervalo_analise.HasValue ? setup.intervalo_analise.Value : 0;
                    horario.IntervaloAnaliseAlarme = setup.intervalo_analise_alarme.HasValue ? setup.intervalo_analise_alarme.Value : 0;
                    horario.ContaEnvios = setup.conta_envios.HasValue ? setup.conta_envios.Value : 0;
                    horario.DiaEnvioRelat = setup.dia_envio_relat;
                    horario.HoraEnvioRelat = setup.hora_envio_relat;
                    horario.DiasRun = setup.dias_run;
                    horario.FimTurno = setup.fim_turno;
                    horario.InicioTurno = setup.inicio_turno;
                    horario.t_card_normal = setup.t_card_normal.HasValue ? setup.t_card_normal.Value : 0;

                    horario.HorariosEnviosCard = new List<HorariosEnviosCard>();
                    setup.DeviceConfigurationHorariosEnviosCard = _DeviceConfigurationService.GetHoras(setup.Id);
                    foreach (var hora in setup.DeviceConfigurationHorariosEnviosCard)
                    {
                        horario.HorariosEnviosCard.Add(new HorariosEnviosCard()
                        {
                            Hora = hora.Hora != null ? hora.Hora.Substring(0, 5) : ""
                        });
                    }

                    gatilho.Config = Convert.ToInt32(setup.config);
                    gatilho.RmsAccRed = setup.rms_acc_red.HasValue ?
                        Decimal.ToDouble(setup.rms_acc_red.Value) : 0;
                    gatilho.RmsAccYel = setup.rms_acc_yel.HasValue ?
                        Decimal.ToDouble(setup.rms_acc_yel.Value) : 0;
                    gatilho.MinRmsAcc = setup.min_rms_acc.HasValue ?
                        Decimal.ToDouble(setup.min_rms_acc.Value) : 0;
                    gatilho.RmsSpdRed = setup.rms_spd_red.HasValue ?
                        Decimal.ToDouble(setup.rms_spd_red.Value) : 0;
                    gatilho.RmsSpdYel = setup.rms_spd_yel.HasValue ?
                        Decimal.ToDouble(setup.rms_spd_yel.Value) : 0;
                    gatilho.MaxVar = setup.max_var.HasValue ? setup.max_var.Value : 0;

                    var loraSetupData = defaultLora;
                    lora.Canal = loraSetupData.canal.Value;
                    lora.End = loraSetupData.end.Value;
                    lora.Syn = loraSetupData.syn.Value;
                    lora.Sf = loraSetupData.sf.Value;
                    lora.Bw = loraSetupData.bw.Value;
                    lora.Gtw = loraSetupData.gtw.Value;
                }

                var device = _DeviceService.Get(deviceConfig.Key.Value);
                if (device != null && deviceData.Id == device.Code)
                {
                    response.Sensor.Add(sensor);
                    response.Horarios.Add(horario);
                    response.Gatilhos.Add(gatilho);
                    response.Lora.Add(lora);
                    response.Versao = "1.3.1";
                    response.Sn = device.Code;

                    deviceConfigEnviados.Add(deviceConfigId);

                    var receiveData = GenerateEntityFromDeviceDataRequest(deviceData, deviceConfigId);
                    _ReceiveService.InsertData(receiveData);
                }
            }

            if (deviceConfigEnviados.Any() == false)
            {
                Lora lora = new Lora();
                lora.Canal = defaultLora.canal.Value;
                lora.End = defaultLora.end.Value;
                lora.Syn = defaultLora.syn.Value;
                lora.Sf = defaultLora.sf.Value;
                lora.Bw = defaultLora.bw.Value;
                lora.Gtw = defaultLora.gtw.Value;

                AddEmptySensorToResponse(response, lora, deviceData.Id);
            }

            respJson = JsonConvert.SerializeObject(response);

            ContentResult contRes = Content(Newtonsoft.Json.Linq.JObject.Parse(respJson).ToString(), "application/json");
            contRes.StatusCode = (int)HttpStatusCode.OK;

            foreach (var dcId in deviceConfigEnviados)
            {
                var dc = _DeviceConfigurationService.Get(dcId);
                if (dc != null)
                {
                    dc.config = false;
                    _DeviceConfigurationService.Edit(dc, null);
                }
            }

            return contRes;
        }

        private ReceiveData GenerateEntityFromDeviceDataRequest(DeviceDataRequest deviceData, int idDeviceConfiguration)
        {
            return new ReceiveData()
            {
                id = deviceData.Id,
                IdDeviceConfiguration = idDeviceConfiguration,
                gtw = deviceData.gtw,
                seq = deviceData.seq,
                alarme = deviceData.alarme,
                dec = deviceData.dec,
                ftr_crista = deviceData.ftr_crista,
                rms_acc = deviceData.rms_acc,
                rms_spd = deviceData.rms_spd,
                setup_amostras = deviceData.setup != null && deviceData.setup.Any() ?
                    deviceData.setup.First().amostras : 0,
                setup_eixo = deviceData.setup != null && deviceData.setup.Any() ?
                    deviceData.setup.First().eixo : 0,
                setup_filtro = deviceData.setup != null && deviceData.setup.Any() ?
                    deviceData.setup.First().filtro : 0,
                setup_freq_cut = deviceData.setup != null && deviceData.setup.Any() ?
                    deviceData.setup.First().freq_cut : 0,
                setup_fs = deviceData.setup != null && deviceData.setup.Any() ?
                    deviceData.setup.First().fs : 0,
                setup_odr = deviceData.setup != null && deviceData.setup.Any() ?
                    deviceData.setup.First().Odr : 0,
                temp = deviceData.temp,
                tipo = deviceData.tipo,
                dado = deviceData.dado
            };
        }

        #region abc
        [AllowAnonymous]
        [HttpGet("deviceById")]
        public Motor GetDeviceById(int Id)
        {
            return _MotorService.Get(Id);
        }

        [AllowAnonymous]
        [HttpPost("createDevice")]
        public ContentResult AddDevice(MotorModel motor)
        {
            if (motor.CompressorTypeId == 0)
                motor.CompressorTypeId = 1;

            try
            {
                var Motor = _mapper.Map<Motor>(motor);
                Motor.Id = _MotorService.GetlastCode();
                //Motor.Bushing = 
                var idMotor = _MotorService.Insert(Motor);


                var respJson = "{\"Message\":\"Equipamento Cadastrado com sucesso\",\"Id\":\"" + idMotor + "\"}";
                ContentResult contRes1 = Content(Newtonsoft.Json.Linq.JObject.Parse(respJson).ToString(), "application/json");
                contRes1.StatusCode = (int)HttpStatusCode.OK;
                return contRes1;
            }
            catch (Exception e)
            {
                var respJson = "{\"Message\":\"Erro ao adicionar Equipamento\",\"Erro\":\"" + e.Message.ToString() + ":" + e.InnerException.Message.ToString() + "\"}";
                ContentResult contRes1 = Content(Newtonsoft.Json.Linq.JObject.Parse(respJson).ToString(), "application/json");
                contRes1.StatusCode = (int)HttpStatusCode.InternalServerError;
                return contRes1;
            }
        }

        // GET: api/<MembersController>
        [AllowAnonymous]
        [HttpGet("getDetailDevices")]
        public IEnumerable<Motor> GetDetailDevices()
        {
            return _MotorService.GetAll();
        }

        //// GET: api/<MembersController>
        //[AllowAnonymous]
        //[HttpGet("GetMachine")]
        //public List<SelectListItemDTO> GetActuationType()
        //{
        //    return _MachineService.GetQueryDropDownList();
        //}

        // GET: api/<MembersController>
        [AllowAnonymous]
        [HttpGet("getMachine")]
        public List<SelectListItemDTO> GetMachine()
        {
            return _MachineService.GetQueryDropDownList();
        }

        // GET: api/<MembersController>
        [AllowAnonymous]
        [HttpGet("getFixationType")]
        public List<SelectListItemDTO> GetFixationType()
        {
            return _FixationService.GetQueryDropDownList();
        }

        // GET: api/<MembersController>
        [AllowAnonymous]
        [HttpGet("getCoupling")]
        public List<SelectListItemDTO> GetCoupling()
        {
            return _CouplingService.GetQueryDropDownList();
        }

        // GET: api/<MembersController>
        [AllowAnonymous]
        [HttpGet("getCardanShaftType")]
        public List<SelectListItemDTO> GetCardanShaftType()
        {
            return _CardanShaftTypeService.GetQueryDropDownList();
        }


        // GET: api/<MembersController>
        [AllowAnonymous]
        [HttpGet("getPumpType")]
        public List<SelectListItemDTO> GetPumpType()
        {
            return _PumpTypeService.GetQueryDropDownList();
        }


        // GET: api/<MembersController>
        [AllowAnonymous]
        [HttpGet("getCompressorType")]
        public List<SelectListItemDTO> GetCompressorType()
        {
            return _CompressorTypeService.GetQueryDropDownList();
        }

        // GET: api/<MembersController>
        [AllowAnonymous]
        [HttpGet("getDevice")]
        public List<SelectListItemDTO> GetDevice()
        {
            return _DeviceService.GetQueryDropDownList();
        }

        // GET: api/<MembersController>
        [AllowAnonymous]
        [HttpGet("getMotor")]
        public List<SelectListItemDTO> GetMotor()
        {
            List<SelectListItemDTO> listMotor = new List<SelectListItemDTO>
            {
                new SelectListItemDTO(){Key =  1 , Value = "Motor" },
                new SelectListItemDTO() { Key = 2,Value = "Redutores" },
                new SelectListItemDTO() {  Key = 3, Value = "Bomba"},
                new SelectListItemDTO() {  Key = 4,Value = "Ventilador / Exaustor"},
                new SelectListItemDTO(){Key = 5,Value = "Compressor"}
            };

            return listMotor;
        }

        //[AllowAnonymous]
        //// GET: api/<MembersController>
        //[HttpGet]
        //public IEnumerable<Motor> AllMembers()
        //{
        //    ////return lstMember;
        //    return _MotorService.GetAll();
        //}

        //// GET: api/<DeviceController>
        //[AllowAnonymous]
        //[HttpGet]
        //[Produces("application/json")]
        //public ContentResult GetA()
        //{
        //    string respJson = "{\"Status\":\"Active\",\"Connection\":\"Ok\"}";
        //    ContentResult contRes1 = Content(Newtonsoft.Json.Linq.JObject.Parse(respJson).ToString(), "application/json");
        //    return contRes1;
        //}


        //// GET api/<MembersController>/5
        //[HttpGet("{id}")]
        //public Member MemberByid(int id)
        //{
        //    return lstMember.Find(x => x.Id == id);
        //}

        [AllowAnonymous]
        // POST api/<MembersController>
        [HttpPost("authentication")]
        public IActionResult Authentication([FromBody] UserCredential userCredential)
        {
            var token = _jwtAuth.Authentication(userCredential.UserName, userCredential.Password);
            if (token == null)
                return Unauthorized();
            return Ok(token);
        }
        #endregion
    }
}
