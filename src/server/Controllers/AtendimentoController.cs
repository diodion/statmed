using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Statmed.Models;
using System.Text.Json;
using System.Web;
using System.Text.Json.Serialization;

namespace Statmed.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AtendimentoController : Controller
    {
        private readonly StatmedDbContext _statmedDbContext;

        public AtendimentoController(StatmedDbContext context)
        {
            _statmedDbContext = context;
        }

        [HttpGet("Consultar")]
        public async Task<IActionResult> BuscaAtendimento()
        {
            var atendimentos = await _statmedDbContext.Atendimento.ToListAsync();

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };            

            return Ok(JsonSerializer.Serialize(atendimentos, options));
        }

        [HttpPost("Cadastrar")]
        public async Task<ActionResult<Atendimento>> CadastrarAtendimento([FromServices] StatmedDbContext _statmedDbContext, [FromBody] Atendimento body)
        {

            var atendimento = new Atendimento()
            {
                Usuario_idFunc = body.Usuario_idFunc,
                PacienteIdSame = body.PacienteIdSame,
                Data = body.Data,
                Epidemia = body.Epidemia,
            };

            _statmedDbContext.Atendimento.Add(atendimento);
            await _statmedDbContext.SaveChangesAsync();
            return atendimento;
        }

        [HttpGet("BuscaIdAtendimento")]
        public async Task<ActionResult<Atendimento>> BuscaIdAtendimento(int IdAtendimento)
        {
            Atendimento IdPaciente = await _statmedDbContext.Atendimento.Select(s => new Atendimento
            {
                IdAtendimento = s.IdAtendimento,
                Data = s.Data,
                Cid = s.Cid,
                Epidemia = s.Epidemia,
                Atestado = s.Atestado,
                Anamnese = s.Anamnese,
                Relatorio = s.Relatorio,
                Encaminhamento = s.Encaminhamento,
                PacienteIdSame = s.PacienteIdSame,
                Paciente = s.Paciente
            }).FirstOrDefaultAsync(s => s.IdAtendimento == IdAtendimento);
            if (IdPaciente == null)
            {
                return NotFound();
            }
            else
            {
                return IdPaciente;
            }
        }
        [HttpPut("AtualizarAtendimento")]
        public async Task<HttpStatusCode> RegistrarAnamnese(Atendimento Atendimento)
        {
            var attPac = await _statmedDbContext.Atendimento.FirstOrDefaultAsync(s => s.IdAtendimento == Atendimento.IdAtendimento);

            // Escapar os caracteres inválidos no campo Anamnese
            string anamneseEscaped = RemoveInvalidCharacters(Atendimento.Anamnese);

            attPac.Cid = Atendimento.Cid;
            attPac.Atestado = Atendimento.Atestado;
            attPac.Anamnese = anamneseEscaped; // Usar a string escapada
            attPac.Relatorio = Atendimento.Relatorio;
            attPac.Encaminhamento = Atendimento.Encaminhamento;

            await _statmedDbContext.SaveChangesAsync();
            return HttpStatusCode.OK;
        }

        private string RemoveInvalidCharacters(string input)
        {
            // Expressão regular para remover caracteres inválidos (\x00-\x08, \x0B-\x0C, \x0E-\x1F)
            string pattern = @"[\x00-\x08\x0B-\x0C\x0E-\x1F]";
            string formatted = Regex.Replace(input, pattern, string.Empty);

            return formatted;
        }
    }
}