using ApiFuncional.Data;
using ApiFuncional.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiFuncional.Controllers;

[Authorize]//Se usuário esta LOGADO, permite acessar esse controller
[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly ApiDbContext _context;

    public ProdutosController(ApiDbContext context)
    {
        _context = context;
    }
    
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
    {
        if (!_context.Produtos.Any())
            return NotFound();
        return await _context.Produtos.ToListAsync();
    }
    
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult<Produto>> GetProduto(int id)
    {
        if (!_context.Produtos.Any())
            return NotFound();
        var produto = await _context.Produtos.FindAsync(id);
        if (produto == null)
            return NotFound();
        return produto;
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult<Produto>> PostProduto(Produto produto)
    {
        
        if (!ModelState.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(ModelState)
            {
                Title = "Um ou mais erros de validação ocorreram."
            });
        }
        
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProduto), new { id = produto.Id }, produto);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult> PutProduto(int id, Produto produto)
    {
        if (id != produto.Id)
            return BadRequest();
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        
        // _context.Produtos.Update(produto);//Pode gerar problema, pois tenta atachar o objeto na memória
        
        _context.Entry(produto).State = EntityState.Modified;//Mais seguro

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            if (!ProdutoExists(id))
                return NotFound();
            throw;
        }
        
        return NoContent();
    }

    private bool ProdutoExists(int id)
    {
        return (_context.Produtos?.Any(e => e.Id == id)).GetValueOrDefault();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult> DeleteProduto(int id)
    {
        if (_context.Produtos == null)
            return NotFound();
        
        var produto = await _context.Produtos.FindAsync(id);
        if (produto == null)
            return NotFound();
        
        _context.Produtos.Remove(produto);
        await _context.SaveChangesAsync();
        return NoContent();
    }
    
    
}