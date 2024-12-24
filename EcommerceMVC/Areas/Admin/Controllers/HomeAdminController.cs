using EcommerceMVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;


namespace EcommerceMVC.Areas.Admin.Controllers
{

    [Area("admin")]
    [Route("admin")]
    [Route("admin/homeadmin")]
    [Authorize(Roles = "Admin")]
    public class HomeAdminController : Controller
    {
        private readonly Hshop2023Context db;

        public HomeAdminController(Hshop2023Context context)
        {
            db = context;
        }

        [Route("")]
        [Route("index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("Danhmucsanpham")]
        //public IActionResult DanhMucSanPham()
        //{
        //	var lstSanPham = db.HangHoas.ToList();
        //	return View(lstSanPham);

        //}

        public IActionResult DanhMucSanPham(int? page)
        {
            int pageSize = 12;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var lstsanpham = db.HangHoas.AsNoTracking().OrderBy(x => x.MaHh);
            IPagedList<HangHoa> lst = new PagedList<HangHoa>(lstsanpham, pageNumber, pageSize);
            return View(lst);

        }
        [Route("ThemSanPhamMoi")]
        [HttpGet]
        public IActionResult ThemSanPhamMoi()
        {
            var danhSachLoai = db.Loais.ToList();
            var danhSachNCC = db.NhaCungCaps.ToList();

            // Kiểm tra nếu không có dữ liệu
            if (!danhSachLoai.Any() || !danhSachNCC.Any())
            {
                // Trả về một lỗi hoặc thông báo cho người dùng
                ViewBag.ErrorMessage = "Danh sách loại hoặc nhà cung cấp không có dữ liệu.";
            }
            else
            {
                ViewBag.MaLoai = new SelectList(danhSachLoai, "MaLoai", "TenLoai");
                ViewBag.MaNCC = new SelectList(danhSachNCC, "MaNcc", "TenCongTy");
            }
            return View();
        }
        [Route("ThemSanPhamMoi")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemSanPhamMoi(HangHoa sanPham)
        {

            if (!ModelState.IsValid)
            {
                db.HangHoas.Add(sanPham);
                db.SaveChanges();
                return RedirectToAction("DanhMucSanPham");

            }
            return View(sanPham);
        }

        [Route("SuaSanPham")]
        [HttpGet]
        public IActionResult SuaSanPham(int maSanPham)  // Thay đổi tham số sang kiểu int
        {
            // Tìm sản phẩm theo mã số
            var sanPham = db.HangHoas.Find(maSanPham);  // Không cần kiểm tra kiểu dữ liệu vì đã là int
            if (sanPham == null)
            {
                return NotFound("Không tìm thấy sản phẩm.");
            }

            // Tạo ViewBag cho danh sách loại và nhà cung cấp
            ViewBag.MaLoai = new SelectList(db.Loais.ToList(), "MaLoai", "TenLoai");
            ViewBag.MaNCC = new SelectList(db.NhaCungCaps.ToList(), "MaNcc", "TenCongTy");

            return View(sanPham);
        }

        [Route("SuaSanPham")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaSanPham(HangHoa sanPham)
        {
            // Debug: Kiểm tra các lỗi nếu có
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            // Kiểm tra tính hợp lệ của model
            if (sanPham!=null)
            {
                //var existingProduct = db.HangHoas.Find(sanPham.MaHh);
                //existingProduct.TenHh = sanPham.TenHh;
                //existingProduct.DonGia = sanPham.DonGia;
                // Cập nhật sản phẩm trong cơ sở dữ liệu
                db.Entry(sanPham).State = EntityState.Modified;  // Đảm bảo trạng thái là "Modified"
                db.SaveChanges();

                // Chuyển hướng đến danh mục sản phẩm
                return RedirectToAction("DanhMucSanPham");
            }

            // Nếu model không hợp lệ, trả về view với đối tượng sản phẩm
            return View(sanPham);
        }
        //[Route("SuaSanPham")]
        //[HttpGet]
        //public IActionResult SuaSanPham(int maSanPham)
        //{
        //    var sanPham = db.HangHoas.Find(maSanPham);
        //    if (sanPham == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(sanPham); // Trả về View để người dùng có thể sửa sản phẩm
        //}
        //[Route("SuaSanPham")]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult SuaSanPham(HangHoa sanPham)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var existingProduct = db.HangHoas.Find(sanPham.MaHh);
        //        if (existingProduct == null)
        //        {
        //            return NotFound();
        //        }

        //        // Cập nhật thông tin sản phẩm
        //        existingProduct.TenHh = sanPham.TenHh;
        //        existingProduct.DonGia = sanPham.DonGia;
        //        // Cập nhật các trường khác tùy theo yêu cầu

        //        db.SaveChanges();
        //        return RedirectToAction("DanhMucSanPham");  // Quay lại trang danh mục sản phẩm
        //    }

        //    // Nếu có lỗi, trả về view với dữ liệu sản phẩm hiện tại
        //    return View(sanPham);
        //}


        [Route("XoaSanPham")]
        [HttpPost]
        [ValidateAntiForgeryToken]  // Đảm bảo bảo mật CSRF
        public IActionResult XoaSanPham(int maSanPham)
        {
            // Tìm sản phẩm theo mã sản phẩm (maSanPham)
            var hangHoa = db.HangHoas.Find(maSanPham);
            if (hangHoa == null)
            {
                // Nếu không tìm thấy sản phẩm, trả về lỗi
                return NotFound("Không tìm thấy sản phẩm cần xóa.");
            }

            // Xóa sản phẩm khỏi cơ sở dữ liệu
            db.HangHoas.Remove(hangHoa);
            db.SaveChanges();

            // Sau khi xóa, chuyển hướng về danh mục sản phẩm
            return RedirectToAction("DanhMucSanPham");
        }


    }
}
