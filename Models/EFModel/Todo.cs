﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace ToDoList.Models.EFModel;

public partial class Todo
{
    public int Id { get; set; }

    public string Task { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsComplete { get; set; }
}