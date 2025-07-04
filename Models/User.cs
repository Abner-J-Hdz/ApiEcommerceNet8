﻿using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Models;
public class User
{
	[Key]
	public int Id { get; set; }

	public string? Name { get; set; }
	public string UserName { get; set; } = string.Empty;
	public string? Password { get; set; }
	public string? Role { get; set; }
}
