using Microsoft.AspNetCore.Mvc;
using SmartFoundation.Application.Services;
using SmartFoundation.UI.ViewModels.SmartPage;
using SmartFoundation.UI.ViewModels.SmartTable;
using System.Text.Json;

namespace SmartFoundation.Mvc.Controllers.VIC
{
    public class VehicleController : Controller
    {
        private readonly VehicleService _vehicleService;

        public VehicleController(VehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        public async Task<IActionResult> Index()
        {
            var parameters = new Dictionary<string, object?>();

            var json = await _vehicleService.GetVehicleList(parameters);

            using var doc = JsonDocument.Parse(json);

            var rows = new List<Dictionary<string, object?>>();

            if (doc.RootElement.TryGetProperty("data", out var dataElement) &&
                dataElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in dataElement.EnumerateArray())
                {
                    var row = new Dictionary<string, object?>();

                    foreach (var prop in item.EnumerateObject())
                    {
                        row[prop.Name] = prop.Value.ValueKind switch
                        {
                            JsonValueKind.String => prop.Value.GetString(),
                            JsonValueKind.Number => prop.Value.TryGetInt64(out var l) ? l : prop.Value.GetDouble(),
                            JsonValueKind.True => true,
                            JsonValueKind.False => false,
                            JsonValueKind.Null => null,
                            _ => prop.Value.ToString()
                        };
                    }

                    rows.Add(row);
                }
            }
            var headerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["vehicleID"] = "رقم المركبة",
                ["chassisNumber"] = "رقم الشاصي",
                ["ownerID_FK"] = "المالك",
                ["plateLetters"] = "حروف اللوحة",
                ["plateNumbers"] = "أرقام اللوحة",
                ["armyNumber"] = "الرقم العسكري",
                ["yearModel"] = "الموديل",
                ["entryDate"] = "تاريخ الإدخال",
                ["CurrentUserID"] = "المستخدم الحالي",
                ["CustodyStartDate"] = "تاريخ بداية العهدة"
            };
            var columns = new List<TableColumn>();

            if (rows.Count > 0)
            {
                foreach (var key in rows[0].Keys)
                {
                    columns.Add(new TableColumn
                    {
                        Field = key,
                        Label = headerMap.TryGetValue(key, out var arabicName) ? arabicName : key,
                        Type = "text",
                        Sortable = true,
                        Visible = true
                    });
                }
            }
            var tableModel = new SmartTableDsModel
            {
                Columns = columns,
                Rows = rows,
                RowIdField = "vehicleID",
                PageSize = 10,
                PageSizes = new List<int> { 10, 25, 50, 100 },
                QuickSearchFields = new List<string> { "chassisNumber", "plateNumbers", "armyNumber" },
                Searchable = true,
                AllowExport = true,
                PageTitle = "المركبات",
                PanelTitle = "قائمة المركبات",
                EnableCellCopy = true,
                
                Toolbar = new TableToolbarConfig
                {
                    ShowRefresh = true,
                    ShowColumns = true,
                    ShowExportCsv = false,
                    ShowExportExcel = false,

                    ShowAdd = true,
                    ShowAdd1 = true,
                    ShowEdit = true,
                    ShowDelete = true,
                    ShowBulkDelete = false,

                    Add = new TableAction
                    {
                        Label = "إضافة مركبة",
                        Icon = "fa fa-plus",
                        Color = "success",
                        OnClickJs = "alert('صفحة إضافة مركبة لاحقًا');"
                    },

                    Add1 = new TableAction
                    {
                        Label = "عرض التفاصيل",
                        Icon = "fa fa-file-lines",
                        Color = "secondary",
                        OnClickJs = "alert('صفحة التفاصيل لاحقًا');"
                    },

                    Edit = new TableAction
                    {
                        Label = "تعديل بيانات مركبة",
                        Icon = "fa fa-pen",
                        Color = "primary",
                        RequireSelection = true,
                        MinSelection = 1,
                        MaxSelection = 1,
                        OnClickJs = "alert('صفحة التعديل لاحقًا');"
                    },

                    Delete = new TableAction
                    {
                        Label = "حذف بيانات مركبة",
                        Icon = "fa fa-trash",
                        Color = "danger",
                        RequireSelection = true,
                        MinSelection = 1,
                        MaxSelection = 1,
                        OnClickJs = "alert('الحذف لاحقًا');"
                    }
                }
            };

            var vm = new SmartPageViewModel
            {
                PageTitle = "المركبات",
                PanelTitle = "قائمة المركبات",
                PanelIcon = "fa fa-car",
                TableDS = tableModel
            };

            return View(vm);
        }
    }
}