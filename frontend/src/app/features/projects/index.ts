// Project Management Components
export { ProjectListComponent } from './project-list/project-list.component';
export { KanbanBoardComponent } from './kanban-board/kanban-board.component';
export { TaskCardComponent } from './task-card/task-card.component';
export { TaskModalComponent } from './task-modal/task-modal.component';
export { ProjectProgressComponent } from './project-progress/project-progress.component';

// Re-export models and services for convenience
export * from '../../models/project.models';
export { ProjectService } from '../../services/project.service';