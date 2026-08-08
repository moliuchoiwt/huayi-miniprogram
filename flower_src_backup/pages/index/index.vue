<template>
	<view v-if="bannerObj['10002']">
		<image class="pageBg" :src="bannerObj['10002'].imgUrl" mode="widthFix"></image>
		<view class="box">
			<up-navbar title=" " bgColor='#ffffff00' title-color="#000" leftIcon='' :fixed="false" placeholder>
				<template #left>
					<!-- <image class="logo" src="/static/logoLine.png" mode="heightFix"></image> -->
					<image class="logo" src="/static/logo.png" mode="heightFix"></image>
				</template>
			</up-navbar>
			<view class="search">
				<up-search placeholder="搜索" bgColor='#fff' :clearabled="false" :showAction="false" height="88rpx"
					shape="square" @search="toList"></up-search>
			</view>
			<view class="swiper">
				<up-swiper :list="bannerList" height="328rpx" keyName="imgUrl" indicator indicatorMode="dot" circular
					@click="clickSwpier"></up-swiper>
			</view>
			<view class="menus">
				<view class="menu" @click="href('/pagesPerson/info', true)">
					<view class="texts">
						<view class="name">信息填写</view>
						<view class="text">个人信息完善</view>
					</view>
					<image src="/static/icon01.png"></image>
				</view>
				<view class="menu" @click="href('/pagesShop/shop/add', true)" v-if="!MpWxOpenCheck">
					<view class="texts">
						<view class="name">花店入驻</view>
						<view class="text">入驻发布任务</view>
					</view>
					<image src="/static/icon02.png"></image>
				</view>
			</view>
			<view style="margin:32rpx 0;">
				<u-title>任务订单</u-title>
			</view>
			<view class="list">
				<view class="item" v-for="item in list" :key="item.Id"
					@click="href(`/pagesShop/task/detail?id=${item.Id}`)">
					<view class="header">
						<image :src="item.goodsImgList[0]" mode="aspectFill"></image>
						<view class="name">
							<view class="text">
								{{ item.relatedDemand }}
							</view>
						</view>
					</view>
					<view class="info">
						<view class="left">
							<view class="time">
								<view>
									<image src="/static/icon04.png"></image>
								</view>
								<view class="text">收货时间{{ item.receivingTime }}</view>
							</view>
							<view class="business">
								<view class="shop">
									<image class="icon" src="/static/icon03.png" mode="widthFix"></image>
									<view class="name">{{ item.shopName }}</view>
								</view>
								<view class="createTime">
									<uni-dateformat :date="new Date(item.createTime)" :threshold="[60000, 3600000]"
										format="yyyy-MM-dd hh"></uni-dateformat>
									·发布
								</view>
							</view>
						</view>
						<view class="right">
							<view class="price">￥{{ item.price }}</view>
							<view class="btn">申请接单</view>
						</view>
					</view>
				</view>
			</view>
			<view class="emptyBlock" v-if="list.length <= 0">
				<image src="/static/img32.png" mode="widthFix"></image>
			</view>
		</view>
		<up-overlay :show="show" @click="show = false">
			<view class="overlay">  
				<view class="body" v-if="bannerObj['10005']"> 
					<image class="bg" :src="bannerObj['10005'].imgUrl"></image>
				</view>
			</view>
		</up-overlay>
	</view>
</template> 
 
<script>
export default {
	data() {
		return {
			bannerList: [],
			bannerObj: {},
			list: [], 
			show: true,
			query: {
				pageNum: 1,
				pageSize: 10,
				queryName: ""
			},
			total: 0,
			MpWxOpenCheck: true
		}
	},
	onLoad() {
		// 任务订单
		this.getList()
		// 获取静态资源路径
		this.request('Other/GetConfig').then(res => {
			this.MpWxOpenCheck = res.data.MpWxOpenCheck
		})
	},
	onPullDownRefresh() {
		this.getList(true).then(() => {
			uni.stopPullDownRefresh()
		})
	},
	onShow() {
		// 轮播图
		this.request(`news/BannerList`, {
			pageNum: 1,
			pageSize: 10,
		}).then(res => {
			this.bannerList = res.data.items
		})
		// 图片  
		this.request(`news/BannerList`, {
			Ids: [10002, 10005]
		}).then(res => {
			this.bannerObj = res.data.items
		})
		this.getList()
	},
	async onReachBottom() {
		if (this.total > this.query.pageNum * this.query.pageSize) {
			this.query.pageNum += 1
			await this.getList()
		} else {
			this.toast('没有更多了')
		}
	},
	methods: {
		//点击轮播图
		clickSwiper(e) {
			this.href(this.bannerList[e].Link)
		},
		toList(e) {
			this.href(`/pagesShop/task/list?queryName=${e}`)
		},
		async getList(reset) {
			if (reset) this.query.pageNum = 1
			let res = await this.request('Order/TaskList', this.query)
			if (this.query.pageNum == 1) {
				this.list = res.data.items
				this.total = res.data.total
			} else {
				this.list.push(...res.data.items)
			}
		},
	}
}
</script>

<style lang="scss" scoped>
:deep(.u-navbar__content__left) {
	padding: 0 !important;
}

.pageBg {
	width: 750rpx;
}

.box {
	position: absolute;
	left: 0;
	top: 0;
	width: 750rpx;
	padding: 0 28rpx;

	.logo {
		height: 80rpx;
	}

	.search {
		margin-top: 32rpx;
	}

	.swiper {
		margin-top: 32rpx;
	}

	.menus {
		margin-top: 24rpx;
		display: flex;
		justify-content: space-between;

		.menu {
			display: flex;
			justify-content: space-between;
			background: #ffffff;
			border-radius: 16rpx;
			padding: 32rpx;
			width: 100%;

			.texts {
				.name {
					font-weight: 600;
					font-size: 32rpx;
					color: #333333;
				}

				.text {
					font-size: 20rpx;
					color: #666666;
					margin-top: 10rpx;
				}
			}

			image {
				width: 88rpx;
				height: 88rpx;
			}
		}

		.menu:not(:nth-child(1)) {
			margin-left: 26rpx;
		}
	}

	.list {
		.item {
			margin-bottom: 24rpx;
			background: #FFFFFF;
			border-radius: 16rpx;
			padding: 24rpx;

			.header {
				display: flex;

				image {
					width: 128rpx;
					height: 128rpx;
					border-radius: 8rpx;
				}

				.name {
					flex: 1;
					margin-left: 20rpx;
					font-size: 32rpx;
					color: #333333;

					.text {
						overflow: hidden;
						text-overflow: ellipsis;
						display: -webkit-box;
						-webkit-box-orient: vertical;
						-webkit-line-clamp: 2;
					}
				}
			}

			.info {
				display: flex;
				justify-content: space-between;
				margin-top: 16rpx;

				.left {
					flex: 1;

					.time {
						display: flex;
						align-items: center;
						padding: 14rpx 18rpx;
						background: linear-gradient(90deg, #F5F5F5 70%, rgba(245, 245, 245, 0) 100%);

						image {
							width: 32rpx;
							height: 32rpx;
						}

						.text {
							font-size: 24rpx;
							color: #666666;
							margin-left: 6rpx;
						}
					}

					.business {
						flex: 1;
						display: flex;
						justify-content: space-between;
						align-items: center;
						margin-top: 40rpx;

						.shop {
							display: flex;
							align-items: center;

							.icon {
								width: 44rpx;
								height: 44rpx;
							}

							.name {
								white-space: nowrap;
								margin-left: 6rpx;
								font-size: 24rpx;
								color: #333333;
							}
						}

						.createTime {
							padding-right: 56rpx;
							font-size: 24rpx;
							color: #666666;
						}
					}
				}

				.right {
					display: flex;
					flex-direction: column;
					align-items: flex-end;

					.price {
						font-weight: 600;
						font-size: 40rpx;
						color: #DE4343;
					}

					.btn {
						margin-top: 16rpx;
						width: 192rpx;
						height: 64rpx;
						background: #262626;
						border-radius: 16rpx;
						display: flex;
						align-items: center;
						justify-content: center;
						font-size: 28rpx;
						color: #FFFFFF;
					}
				}
			}
		}
	}
}

.overlay {
	width: 100%;
	height: 100vh;
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	padding: 0 72rpx;

	.body {
		position: relative;

		.bg {
			width: 606rpx;
			height: 606rpx;
		}

	}

}
</style>