import { Component } from '@angular/core';
import { AdminIcon } from '../../components/admin-icon/admin-icon';

interface LandlordSubmission {
  name: string;
  email: string;
  initials: string;
  submitted: string;
  identity: string;
  deed: string;
  liveness: string;
  state: 'Pending' | 'Verified' | 'Rejected';
}

@Component({
  selector: 'app-admin-landlord-reviews',
  standalone: true,
  imports: [AdminIcon],
  templateUrl: './landlord-reviews.html',
  styleUrl: './landlord-reviews.scss',
})
export class LandlordReviews {
  searchQuery = '';

  readonly submissions: LandlordSubmission[] = [
    { name: 'Marcus Vance', email: 'marcus.vance@email.com', initials: 'MV', submitted: 'Today, 09:14', identity: 'Verified ID', deed: 'Travis County match', liveness: '99.4%', state: 'Pending' },
    { name: 'Elena Rostova', email: 'elena@rostovarealestate.io', initials: 'ER', submitted: 'Yesterday, 17:40', identity: "Driver's license", deed: 'Multi-party title', liveness: '97.8%', state: 'Pending' },
    { name: 'Brandon Cole', email: 'b.cole.rentals99@gmail.com', initials: 'BC', submitted: 'Yesterday, 14:12', identity: 'Expired ID', deed: 'Unmatched grantor', liveness: 'Mismatch', state: 'Pending' },
  ];

  updateStatus(submission: LandlordSubmission, state: 'Verified' | 'Rejected'): void {
    submission.state = state;
  }

  get filteredSubmissions(): LandlordSubmission[] {
    const query = this.searchQuery.trim().toLocaleLowerCase();
    return this.submissions.filter((submission) =>
      `${submission.name} ${submission.email}`.toLocaleLowerCase().includes(query),
    );
  }

  setSearchQuery(event: Event): void {
    if (event.target instanceof HTMLInputElement) {
      this.searchQuery = event.target.value;
    }
  }
}
