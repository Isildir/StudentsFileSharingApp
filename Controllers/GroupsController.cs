using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentsFileSharingApp.Dtos;
using StudentsFileSharingApp.Entities;
using StudentsFileSharingApp.Entities.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentsFileSharingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly BasicContext context;

        public GroupsController(BasicContext context)
        {
            this.context = context;
        }

        /*
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSkill(int id)
        {
            var ability = await context.Set<Skill>().FindAsync(id);

            if (ability == null)
                return NotFound();

            context.Set<Skill>().Remove(ability);

            await context.SaveChangesAsync();

            return NoContent();
        }
        */

        [HttpGet("{id}")]
        public async Task<ActionResult<GroupDto>> GetSkill(int id)
        {
            var item = await context.Set<Group>().FindAsync(id);

            if (item == null)
                return NotFound();

            return new GroupDto { Id = item.Id, Name = item.Name };
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupDto>>> GetSkills()
        {
            var entities = await context.Set<Group>().ToListAsync();

            return Ok(entities.Select(a => new GroupDto { Id = a.Id, Name = a.Name }));
        }

        /*
        [HttpPost]
        public async Task<ActionResult<SkillDto>> PostSkill(SkillDto skill)
        {
            if (skill == null)
                return BadRequest();

            var entity = new Skill { Name = skill.Name };

            context.Set<Skill>().Add(entity);

            await context.SaveChangesAsync();

            return CreatedAtAction("GetAbility", new { id = skill.Id }, skill);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSkill(int id, SkillDto skill)
        {
            if (id != skill.Id)
                return BadRequest();

            var entity = await context.Set<Skill>().FindAsync(id);

            if (entity == null)
                return NotFound();

            entity.Name = skill.Name;

            await context.SaveChangesAsync();

            return NoContent();
        }*/
    }
}