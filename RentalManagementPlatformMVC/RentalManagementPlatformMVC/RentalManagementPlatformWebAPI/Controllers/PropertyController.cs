using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs.Property;
using RentalManagementPlatformWebAPI.Services.Property.Interfaces;

namespace RentalManagementPlatformWebAPI.Controllers
{

	[ApiController]
	[Route("api/[controller]")]
	public class PropertyController: ControllerBase
	{
		private readonly IPropertyService _service;
		public PropertyController(IPropertyService service)
		{
			_service = service;
		}

		//☆建立公告
		[HttpPost("create")]//Post api/property/create
		public async Task<IActionResult> Create([FromBody] PostCreateDto dto) 
		{
			//驗證前端送進來的資料是否符合規範
			if (!ModelState.IsValid) 
			{
				return BadRequest(ModelState);
			}
			//呼叫Service層建立公告
			var postId = await _service.CreatePostAsync(dto);
			//回傳建立成功的結果(並建立公告ID)
			return Ok(new { success = true, PostId = postId });
		}

		//☆公告列表
		[HttpPost("list")]//Post api/property/list
		public async Task<IActionResult> GetAll() 
		{
			//呼叫Service層取得所有公告列表
			var posts = await _service.GetAllPostsAsync();
			//回傳公告列表
			return Ok(posts);
		}

		//☆公告單筆詳細資訊
		[HttpGet("{postId}")]//Get api/property/{postId}
		public async Task<IActionResult> GetById(int postId) 
		{
			//呼叫Service層根據ID取得公告詳細資訊
			var post = await _service.GetPostByIdAsync(postId);
			//如果公告不存在,回傳404 Not Found
			if (post == null) 
			{
				return NotFound();
			}
			//回傳公告詳細資訊
			return Ok(post);
		}

		//☆更新修改公告
		[HttpPut("update")]//Put api/property/update
		public async Task<IActionResult> Update([FromBody] PostUpdateDto dto) 
		{
			//呼叫service層更新公告
			var result = await _service.UpdatePostAsync(dto);
			//如果更新失敗,回傳404 Not Found
			if (!result)return NotFound();
			//回傳更新成功結果
			return Ok(new { success = true });
		}

		//☆刪除公告
		[HttpDelete("{postId}")]//Delete api/property/{postId}
		public async Task<IActionResult> Delete(int postId) 
		{
			//呼叫service層刪除公告
			var result = await _service.DeletePostAsync(postId);
			//如果刪除失敗,回傳404 Not Found
			if (!result) return NotFound();
			//回傳刪除成功結果
			return Ok(new { success = true });
		}
	}
}
