using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGEP.Data;
using SGEP.Models;

namespace SGEP.Controllers
{
    [AllowAnonymous]
    public class AlmoxarifadoController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AlmoxarifadoController(ApplicationDbContext context) => _context = context;
        public IActionResult Index() => View();
        public async Task<JsonResult> List(string id, string nome, string projeto, int? itensPorPagina, int? pagina)
        {
            IEnumerable<Almoxarifado> result = await _context.Almoxarifado.ToListAsync();
            if (id != null && id.Trim() != "")
                result = result.Where(m => m.Id.ToString().Contains(id));
            if (nome != null && nome?.Trim() != "")
                result = result.Where(m => m.Nome.Contains(nome));
            if (projeto != null && projeto?.Trim() != "")
                result = result.Where(m => m.Projeto.ToString().Contains(projeto));
            int inicio = (itensPorPagina ?? 10)*((pagina ?? 1) - 1);
            int qtd = Math.Min (itensPorPagina ?? 10, result.Count() - inicio);
            result = result.ToList().GetRange(inicio, qtd);
            
            return Json(new {size = _context.Almoxarifado.Count(), entities = result.ToList()});
        }
        public async Task<JsonResult> All() => Json(await _context.Almoxarifado.ToListAsync());
        [HttpGet("/Almoxarifado/GetMateriais/{id}")]
        public JsonResult GetMateriais(int id)
        {
            var materiais = _context.Almoxarifado.Find(id).AlmoxarifadosxMateriais.Where(am => am.Quantidade > 0).ToList().ConvertAll(am => _context.Material.Find(am.MaterialId));
            var json = Json(materiais);
            return json;
        } 
        [HttpGet("/Almoxarifado/GetQuantidade/{idAlm}/{idMat}")]
        public JsonResult GetMateriais(int idAlm, int idMat) => Json (_context.Almoxarifado.Find(idAlm)
                                                                                           .AlmoxarifadosxMateriais
                                                                                           .Where(am => am.MaterialId == idMat && am.Quantidade > 0)
                                                                                           .ToList());
        [HttpGet("/Almoxarifado/Get/{id}")]
        public async Task<JsonResult> Get (int id)
        {
            Almoxarifado a = await _context.Almoxarifado.FindAsync(id);
            return Json(a);
        } 
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,Nome,Projeto")] Almoxarifado almoxarifado)
        {
            if (ModelState.IsValid)
            {
                _context.Add(almoxarifado);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Projeto,Ativo")] Almoxarifado almoxarifado)
        {
            if (id != almoxarifado.Id)
                return NotFound();
            if (almoxarifado.Projeto)
                return BadRequest("Projects can only be edited from 'Projects' page");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(almoxarifado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlmoxarifadoExists(almoxarifado.Id))
                        return NotFound();
                    else
                        throw;
                }
                return Ok();
            }
            return BadRequest();
        }
        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            var almoxarifado = await _context.Almoxarifado.FirstAsync(a => a.Id == id);
            if (almoxarifado == null)
                return NotFound();
            if (almoxarifado.Projeto)
                return BadRequest();
            almoxarifado.Ativo = !almoxarifado.Ativo;
            await _context.SaveChangesAsync();
            return Ok();
        }
        private bool AlmoxarifadoExists(int id) => _context.Almoxarifado.Any(e => e.Id == id);
    }
}
