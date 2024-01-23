namespace RoadRegistry.BackOffice;

using System;
using Microsoft.EntityFrameworkCore;

public delegate void ConfigureDbContextOptionsBuilder<TDbContext>(IServiceProvider sp, DbContextOptionsBuilder options) where TDbContext: DbContext;
